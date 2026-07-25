using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a single task: its comic name, the credibility threshold required
    /// to complete it, and the score it awards on completion. Authored as an .asset via the
    /// CreateAssetMenu entry; holds no gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "Task_", menuName = "GmtkCountdown/Task Data")]
    public class TaskData : ScriptableObject
    {
        [SerializeField] private string taskName;
        [SerializeField] private int requiredTime;
        [SerializeField] private int scoreValue;

        /// <summary>The comic task name shown to the player.</summary>
        public string Name => taskName;

        /// <summary>Credibility/time threshold required to complete this task.</summary>
        public int RequiredTime => requiredTime;

        /// <summary>Points awarded on completion of this task.</summary>
        public int ScoreValue => scoreValue;
    }
}
