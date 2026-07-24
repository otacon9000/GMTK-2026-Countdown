using System.Collections.Generic;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Owns credibility accumulation, category-repeat penalty, and endless task threshold
    /// progression. There is no victory condition: tasks continue indefinitely, each one
    /// harder than the last. The run only ends when the deck runs out of fragments before
    /// the current task's threshold is reached; currentTaskIndex at that point is the score.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private List<int> taskThresholds = new List<int> { 50, 70, 90 };
        [SerializeField] private int thresholdIncrementPastList = 20;

        private int currentTaskIndex;
        private int accumulatedCredibility;
        private FragmentCategory? lastPlayedCategory;

        /// <summary>Tasks completed so far in this run (0-indexed).</summary>
        public int CurrentTaskIndex => currentTaskIndex;

        /// <summary>Credibility accumulated toward the current task's threshold.</summary>
        public int AccumulatedCredibility => accumulatedCredibility;

        /// <summary>
        /// Credibility threshold for the current task. Taken directly from
        /// <see cref="taskThresholds"/> while in bounds; past the list, extends the last
        /// entry by <see cref="thresholdIncrementPastList"/> per task beyond the list.
        /// </summary>
        public int CurrentThreshold
        {
            get
            {
                if (currentTaskIndex < taskThresholds.Count)
                {
                    return taskThresholds[currentTaskIndex];
                }

                int stepsPastList = currentTaskIndex - taskThresholds.Count + 1;
                int lastThreshold = taskThresholds[taskThresholds.Count - 1];
                return lastThreshold + stepsPastList * thresholdIncrementPastList;
            }
        }

        /// <summary>
        /// Applies a fragment's credibility to the current task, halving it if its category
        /// repeats the previous play. Returns the effective (post-penalty) value.
        /// </summary>
        public int PlayFragment(FragmentData fragment)
        {
            int effectiveValue = fragment.CredibilityValue;

            if (lastPlayedCategory.HasValue && lastPlayedCategory.Value == fragment.Category)
            {
                effectiveValue /= 2;
            }

            accumulatedCredibility += effectiveValue;
            lastPlayedCategory = fragment.Category;

            return effectiveValue;
        }

        /// <summary>
        /// Decides whether the current task is complete, the run has ended, or play continues,
        /// and drives the corresponding GameManager state transition. Call once right after
        /// <see cref="PlayFragment"/>.
        /// </summary>
        public void EvaluateProgress(DeckManager deckManager)
        {
            if (accumulatedCredibility >= CurrentThreshold)
            {
                currentTaskIndex++;
                accumulatedCredibility = 0;
                lastPlayedCategory = null;
                GameManager.Instance.ChangeState(GameState.TaskCompleted);
            }
            else if (deckManager.IsEmpty)
            {
                GameManager.Instance.ChangeState(GameState.GameOver);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Countdown);
            }
        }
    }
}
