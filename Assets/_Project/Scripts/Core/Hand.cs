using System.Collections.Generic;

namespace GmtkCountdown
{
    /// <summary>
    /// The fragments the player is currently holding: a fixed number of slots, each either empty
    /// or holding one <see cref="FragmentData"/>. A plain C# object with no Unity lifecycle — it
    /// knows nothing about input, UI or game state.
    /// <para>
    /// Every method here is a rule about the hand itself. What an action <i>costs</i> is not its
    /// business: GameplayController owns the countdown price of a redraw and the state checks that
    /// decide when an action is allowed at all, and calls in here only to move fragments around.
    /// The methods return what changed rather than a bare bool, so the caller can report it without
    /// having to look the fragment up again after the fact.
    /// </para>
    /// </summary>
    public class Hand
    {
        private readonly List<FragmentData> slots;

        public Hand(int capacity)
        {
            slots = new List<FragmentData>(new FragmentData[capacity]);
        }

        /// <summary>Total number of slots, filled or not.</summary>
        public int SlotCount => slots.Count;

        /// <summary>True when at least one slot holds a fragment.</summary>
        public bool HasAnyFragment => FilledSlotCount > 0;

        /// <summary>True when at least one slot is empty.</summary>
        public bool HasEmptySlot => FindFirstEmptySlot() >= 0;

        /// <summary>
        /// The fragment held in <paramref name="index"/>, or null both when the slot is empty and
        /// when the index is out of range — callers display an empty slot either way.
        /// </summary>
        public FragmentData GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                return null;
            }

            return slots[index];
        }

        /// <summary>
        /// Throws away the fragment in <paramref name="index"/> and returns it. Returns null
        /// without changing anything when the index is out of range, the slot is already empty,
        /// or this is the last fragment in hand.
        /// </summary>
        public FragmentData Discard(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                return null;
            }

            FragmentData fragment = slots[index];
            if (fragment == null)
            {
                return null;
            }

            if (FilledSlotCount <= 1)
            {
                // Discarding the last card in hand would soft-lock the next Interruption; block it.
                return null;
            }

            slots[index] = null;
            return fragment;
        }

        /// <summary>
        /// Removes the fragment in <paramref name="index"/> and returns it, or null if the index is
        /// out of range or the slot is empty. Unlike <see cref="Discard"/> there is no floor here:
        /// playing the last fragment in hand is allowed, and is how a run normally ends.
        /// </summary>
        public FragmentData Play(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                return null;
            }

            FragmentData fragment = slots[index];
            if (fragment == null)
            {
                return null;
            }

            slots[index] = null;
            return fragment;
        }

        /// <summary>
        /// Draws one fragment from <paramref name="deck"/> into the first empty slot and returns
        /// that slot's index, or -1 if nothing was drawn — no empty slot, or an exhausted deck.
        /// A -1 means the deck was never touched, so the caller can safely skip charging for it.
        /// </summary>
        public int TryDrawIntoEmptySlot(DeckManager deck)
        {
            int emptySlot = FindFirstEmptySlot();
            if (emptySlot < 0 || deck.IsEmpty)
            {
                return -1;
            }

            List<FragmentData> drawn = deck.DrawFragments(1);
            if (drawn.Count == 0)
            {
                return -1;
            }

            slots[emptySlot] = drawn[0];
            return emptySlot;
        }

        /// <summary>
        /// Fills every empty slot from <paramref name="deck"/>, in slot order. Draws only as many
        /// fragments as there are empty slots, and leaves the tail empty if the deck runs out.
        /// </summary>
        public void RefillEmptySlots(DeckManager deck)
        {
            int emptySlots = slots.Count - FilledSlotCount;
            if (emptySlots <= 0)
            {
                return;
            }

            List<FragmentData> drawn = deck.DrawFragments(emptySlots);

            int drawnIndex = 0;
            for (int i = 0; i < slots.Count && drawnIndex < drawn.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = drawn[drawnIndex];
                    drawnIndex++;
                }
            }
        }

        private int FilledSlotCount
        {
            get
            {
                int filled = 0;
                foreach (FragmentData fragment in slots)
                {
                    if (fragment != null)
                    {
                        filled++;
                    }
                }

                return filled;
            }
        }

        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
