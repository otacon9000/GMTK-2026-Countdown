using System.Collections.Generic;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Owns earned-time accumulation, the category-repeat penalty, and the current task's
    /// content-driven requirement. There is no victory condition: tasks continue indefinitely,
    /// each one selected by the player at a Break screen. The run only ends when the deck
    /// runs out of fragments before the current task's requirement is met; TotalScore at
    /// that point is the score.
    /// <para>
    /// Playing a fragment produces one number, "earned time", which is then spent in two
    /// different ways — and they are deliberately not the same quantity:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Task progress</b> — added to <see cref="AccumulatedEarnedTime"/> and compared
    /// against <see cref="CurrentRequiredTime"/>. This is a running total of what the player
    /// has earned, never a measure of elapsed seconds.</item>
    /// <item><b>Countdown duration</b> — how many seconds the boss stays away, handed to
    /// CountdownController by the caller.</item>
    /// </list>
    /// <para>
    /// The two diverge in practice. CountdownController clamps the duration to its own minimum,
    /// so a fragment earning less than that floor still buys a full short round while
    /// contributing only its smaller value to the task. In the other direction, redrawing spends
    /// countdown seconds without touching task progress. Treating either number as "the time
    /// left" is therefore wrong: only one of them is on a clock.
    /// </para>
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private TaskData firstTaskData;
        [SerializeField] private List<TaskData> taskPool = new List<TaskData>();
        [SerializeField] private int maxEarnedTime = 12;

        private TaskData currentTaskData;
        private int tasksCompletedCount;
        private int totalScore;
        private int accumulatedEarnedTime;
        private FragmentCategory? lastPlayedCategory;

        /// <summary>Tasks completed so far in this run.</summary>
        public int TasksCompleted => tasksCompletedCount;

        /// <summary>Total score accumulated across all completed tasks this run.</summary>
        public int TotalScore => totalScore;

        /// <summary>Earned time accumulated toward the current task's requirement.</summary>
        public int AccumulatedEarnedTime => accumulatedEarnedTime;

        /// <summary>Earned time the current task requires to be completed.</summary>
        public int CurrentRequiredTime => currentTaskData.RequiredTime;

        private void Awake()
        {
            if (firstTaskData == null)
            {
                // Left unassigned, the first PlayFragment would fail on CurrentRequiredTime with a
                // bare NullReferenceException that says nothing about which field is missing.
                Debug.LogError($"[TaskManager] '{nameof(firstTaskData)}' is not assigned in the Inspector; the run has no task to work on.", this);
                return;
            }

            currentTaskData = firstTaskData;
        }

        /// <summary>
        /// Applies the time a fragment earns to the current task, halving it if its category
        /// repeats the previous play and capping it. Returns the effective earned time, which
        /// the caller also uses as the next countdown's duration.
        /// </summary>
        public int PlayFragment(FragmentData fragment)
        {
            int earnedTime = fragment.BaseEarnedTime;

            if (lastPlayedCategory.HasValue && lastPlayedCategory.Value == fragment.Category)
            {
                earnedTime /= 2;
            }

            earnedTime = Mathf.Min(earnedTime, maxEarnedTime);

            accumulatedEarnedTime += earnedTime;
            lastPlayedCategory = fragment.Category;

            return earnedTime;
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
            accumulatedEarnedTime = 0;
            lastPlayedCategory = null;
            GameManager.Instance.ChangeState(GameState.Countdown);
        }

        /// <summary>
        /// Banks the current task if the fragment just played met its requirement, and reports
        /// whether it did. Call once right after <see cref="PlayFragment"/>.
        /// <para>
        /// Deciding what happens when the task is *not* complete — carry on or end the run — is
        /// deliberately not done here: that answer depends on the hand and the deck, which this
        /// class cannot see. The caller owns it.
        /// </para>
        /// </summary>
        /// <returns>True if the task was completed and the game is moving to TaskCompleted.</returns>
        public bool TryCompleteCurrentTask()
        {
            if (accumulatedEarnedTime < CurrentRequiredTime)
            {
                return false;
            }

            totalScore += currentTaskData.ScoreValue;
            tasksCompletedCount++;
            accumulatedEarnedTime = 0;
            lastPlayedCategory = null;
            GameManager.Instance.ChangeState(GameState.TaskCompleted);
            return true;
        }
    }
}
