using System.Collections.Generic;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Manages a single-use pool of excuse fragments for a run: fragments are drawn into
    /// a hand for the player to choose from, then permanently removed once played.
    /// No category-repetition or credibility logic here — that belongs to a future system.
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [SerializeField] private List<FragmentData> fullDeck;

        private List<FragmentData> availableFragments;

        private void Awake()
        {
            ResetDeck();
        }

        /// <summary>How many fragments are left in the pool.</summary>
        public int RemainingCount => availableFragments.Count;

        /// <summary>True when no fragments remain in the pool.</summary>
        public bool IsEmpty => RemainingCount <= 0;

        /// <summary>
        /// Returns up to <paramref name="handSize"/> random fragments from the pool without
        /// removing them. The hand is only "on offer" until the player plays a fragment.
        /// </summary>
        public List<FragmentData> DrawHand(int handSize)
        {
            int count = Mathf.Min(handSize, availableFragments.Count);
            return availableFragments.GetRange(0, count);
        }

        /// <summary>
        /// Permanently removes the specified fragment from the pool. Call this after the
        /// player plays it, not when it is merely drawn into a hand.
        /// </summary>
        public void RemoveFragment(FragmentData fragment)
        {
            availableFragments.Remove(fragment);
        }

        /// <summary>Reinitializes the pool as a fresh shuffled copy of the full deck.</summary>
        public void ResetDeck()
        {
            availableFragments = new List<FragmentData>(fullDeck);
            Shuffle(availableFragments);
        }

        private static void Shuffle(List<FragmentData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
