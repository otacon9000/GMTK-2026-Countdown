using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a prompt sentence. The prompt text contains a "{0}" placeholder token
    /// marking where a fragment's text is inserted at runtime (see <see cref="BuildSentence"/>).
    /// Authored as an .asset via the CreateAssetMenu entry.
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
    }
}
