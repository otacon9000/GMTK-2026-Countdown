using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a prompt sentence. The prompt text contains a "{0}" placeholder token
    /// marking where a fragment's text is inserted at runtime (see <see cref="BuildSentence"/>).
    /// Authored as an .asset via the CreateAssetMenu entry.
    /// <para>
    /// Known unfinished state, left as it is on purpose: nothing ever inserts a fragment into the
    /// placeholder. The only caller passes a fixed "_____", so the sentence the player reads is
    /// always the blanked-out version and the excuse they picked is never shown completed.
    /// <see cref="PromptText"/> has no callers at all. Whether the resolved sentence should be
    /// displayed after a play is a design decision, not an oversight to patch — but do not assume
    /// from the shape of this class that it is already happening.
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "Prompt_", menuName = "GmtkCountdown/Prompt Data")]
    public class PromptData : ScriptableObject
    {
        // Convention: promptText must contain a single "{0}" token, replaced by the fragment text.
        [SerializeField] private string promptText;

        /// <summary>The raw prompt text, including its "{0}" fragment placeholder token.</summary>
        public string PromptText => promptText;

        /// <summary>
        /// Builds the full sentence by inserting <paramref name="fragmentText"/> into the
        /// prompt's "{0}" placeholder.
        /// </summary>
        /// <param name="fragmentText">The fragment text to insert.</param>
        /// <returns>The prompt with the placeholder replaced by the fragment text.</returns>
        public string BuildSentence(string fragmentText)
        {
            return promptText.Replace("{0}", fragmentText);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(promptText))
            {
                Debug.LogWarning($"[PromptData] '{name}' has no text: it would show as an empty prompt.", this);
            }
            else if (!promptText.Contains("{0}"))
            {
                Debug.LogWarning($"[PromptData] '{name}' has no \"{{0}}\" placeholder: BuildSentence would return it unchanged, with nowhere for the excuse to go.", this);
            }
        }
#endif
    }
}
