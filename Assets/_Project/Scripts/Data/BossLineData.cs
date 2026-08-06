using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// One thing the boss says when he interrupts the player. One line per asset, the same shape
    /// as <see cref="PromptData"/>: the component that shows them holds a list and picks at
    /// random, so adding a line means adding an asset, never editing code.
    /// </summary>
    [CreateAssetMenu(fileName = "BossLine_", menuName = "GmtkCountdown/Boss Line Data")]
    public class BossLineData : ScriptableObject
    {
        [SerializeField] private string lineText;

        /// <summary>The line as the boss says it.</summary>
        public string Text => lineText;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(lineText))
            {
                Debug.LogWarning($"[BossLineData] '{name}' has no text: the boss would say nothing.", this);
            }
        }
#endif
    }
}
