using System.Collections.Generic;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Single-use pool of excuse fragments for a run. A fragment leaves the pool the moment
    /// it is drawn into the hand, not when it is played: the hand is the only place a drawn
    /// fragment exists, and discarding it loses it for the rest of the run. The pool is never
    /// refilled, so running it dry is what eventually ends the run.
    /// Holds no scoring or category logic — that lives in TaskManager.
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
        /// Draws up to <paramref name="count"/> random fragments from the pool, removing each
        /// one as it's drawn so it can never be drawn again. Returns fewer than <paramref name="count"/>
        /// (possibly none) if the pool doesn't have enough fragments left.
        /// </summary>
        public List<FragmentData> DrawFragments(int count)
        {
            int drawCount = Mathf.Min(count, availableFragments.Count);
            var drawn = new List<FragmentData>(drawCount);

            for (int i = 0; i < drawCount; i++)
            {
                int index = Random.Range(0, availableFragments.Count);
                drawn.Add(availableFragments[index]);
                availableFragments.RemoveAt(index);
            }

            return drawn;
        }

        /// <summary>
        /// Reinitializes the pool as a fresh copy of the full deck, skipping any empty entries
        /// left in the serialized list: a null would occupy a draw and silently cost the player
        /// a redraw for no card. The initial shuffle is not what makes draws random —
        /// <see cref="DrawFragments"/> picks a random index every time, so pool order never
        /// affects what comes out. Called once on Awake; a run is restarted by reloading the
        /// scene, not by calling this.
        /// </summary>
        private void ResetDeck()
        {
            availableFragments = new List<FragmentData>(fullDeck.Count);
            foreach (FragmentData fragment in fullDeck)
            {
                if (fragment != null)
                {
                    availableFragments.Add(fragment);
                }
            }

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
