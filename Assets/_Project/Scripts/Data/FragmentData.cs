using UnityEngine;
using UnityEngine.Serialization;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a single excuse fragment: the text shown to the player, its thematic
    /// category, and how much time it earns when played. Authored as an .asset via the
    /// CreateAssetMenu entry; holds no gameplay logic.
    /// <para>
    /// "Earned time" is the game's single currency, but it is spent in two different ways and
    /// the two are not interchangeable — see TaskManager for the full explanation.
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "Fragment_", menuName = "GmtkCountdown/Fragment Data")]
    public class FragmentData : ScriptableObject
    {
        [SerializeField] private string fragmentText;
        [SerializeField] private FragmentCategory category;

        // Renamed from "credibilityValue" during the post-jam refactoring. The attribute keeps
        // the 40 existing Fragment_*.asset files loading their authored value: each one still
        // holds the old key on disk until Unity rewrites it. Do not remove it until every asset
        // has been re-serialized.
        [FormerlySerializedAs("credibilityValue")]
        [SerializeField] private int baseEarnedTime;

        /// <summary>The excuse fragment text (localized content filled in later).</summary>
        public string Text => fragmentText;

        /// <summary>The thematic category this fragment belongs to.</summary>
        public FragmentCategory Category => category;

        /// <summary>
        /// Seconds of work this fragment earns before any rule is applied. TaskManager halves it
        /// when the category repeats the previous play and caps it, so what the player actually
        /// gets is usually lower than this authored value.
        /// </summary>
        public int BaseEarnedTime => baseEarnedTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(fragmentText))
            {
                Debug.LogWarning($"[FragmentData] '{name}' has no text: it would show as a blank card.", this);
            }

            if (baseEarnedTime <= 0)
            {
                Debug.LogWarning($"[FragmentData] '{name}' earns {baseEarnedTime}s: a fragment worth nothing is playable but pointless.", this);
            }
        }
#endif
    }
}
