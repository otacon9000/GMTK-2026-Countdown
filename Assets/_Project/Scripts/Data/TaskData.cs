using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a single task: its comic name, the earned time required to complete
    /// it, and the score it awards on completion. Authored as an .asset via the CreateAssetMenu
    /// entry; holds no gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "Task_", menuName = "GmtkCountdown/Task Data")]
    public class TaskData : ScriptableObject
    {
        [SerializeField] private string taskName;
        [SerializeField] private int requiredTime;
        [SerializeField] private int scoreValue;

        /// <summary>The comic task name shown to the player.</summary>
        public string Name => taskName;

        /// <summary>
        /// Total earned time the player must accumulate to complete this task. Compared against
        /// TaskManager.AccumulatedEarnedTime, not against elapsed seconds.
        /// </summary>
        public int RequiredTime => requiredTime;

        /// <summary>Points awarded on completion of this task.</summary>
        public int ScoreValue => scoreValue;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                Debug.LogWarning($"[TaskData] '{name}' has no name: the Break screen would offer a blank option.", this);
            }

            if (requiredTime <= 0)
            {
                Debug.LogWarning($"[TaskData] '{name}' requires {requiredTime}s: it would complete on the first fragment played.", this);
            }

            if (scoreValue < 0)
            {
                Debug.LogWarning($"[TaskData] '{name}' awards {scoreValue} points: completing it would lower the run's score.", this);
            }
        }
#endif
    }
}
