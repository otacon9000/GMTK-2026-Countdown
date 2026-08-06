using System.Collections.Generic;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Owns credibility accumulation, category-repeat penalty, and the current task's
    /// content-driven threshold. There is no victory condition: tasks continue indefinitely,
    /// each one selected by the player at a Break screen. The run only ends when the deck
    /// runs out of fragments before the current task's threshold is reached; TotalScore at
    /// that point is the score.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private TaskData firstTaskData;
        [SerializeField] private List<TaskData> taskPool = new List<TaskData>();
        [SerializeField] private int maxEarnedTime = 12;

        private TaskData currentTaskData;
        private int tasksCompletedCount;
        private int totalScore;
        private int accumulatedCredibility;
        private FragmentCategory? lastPlayedCategory;

        /// <summary>Tasks completed so far in this run.</summary>
        public int CurrentTaskIndex => tasksCompletedCount;

        /// <summary>Total score accumulated across all completed tasks this run.</summary>
        public int TotalScore => totalScore;

        /// <summary>Credibility accumulated toward the current task's threshold.</summary>
        public int AccumulatedCredibility => accumulatedCredibility;

        /// <summary>Credibility threshold for the current task.</summary>
        public int CurrentThreshold => currentTaskData.RequiredTime;

        private void Awake()
        {
            if (firstTaskData == null)
            {
                // Left unassigned, the first PlayFragment would fail on CurrentThreshold with a
                // bare NullReferenceException that says nothing about which field is missing.
                Debug.LogError($"[TaskManager] '{nameof(firstTaskData)}' is not assigned in the Inspector; the run has no task to work on.", this);
                return;
            }

            currentTaskData = firstTaskData;
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

            effectiveValue = Mathf.Min(effectiveValue, maxEarnedTime);

            accumulatedCredibility += effectiveValue;
            lastPlayedCategory = fragment.Category;

            return effectiveValue;
        }

        /// <summary>
        /// Returns up to <paramref name="count"/> distinct random entries from the task pool.
        /// If the pool has fewer entries than requested, returns all of them.
        /// </summary>
        public List<TaskData> GetTaskChoices(int count)
        {
            List<TaskData> shuffled = new List<TaskData>(taskPool);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }

            int takeCount = Mathf.Min(count, shuffled.Count);
            return shuffled.GetRange(0, takeCount);
        }

        /// <summary>
        /// Sets the chosen task as current, resets progress toward it, and starts the next
        /// Countdown.
        /// </summary>
        public void SelectTask(TaskData chosen)
        {
            currentTaskData = chosen;
            accumulatedCredibility = 0;
            lastPlayedCategory = null;
            GameManager.Instance.ChangeState(GameState.Countdown);
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
                totalScore += currentTaskData.ScoreValue;
                tasksCompletedCount++;
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
