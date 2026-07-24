using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Data container for a single excuse fragment: the text shown to the player, its thematic
    /// category, and how much credibility it contributes when used. Authored as an .asset via
    /// the CreateAssetMenu entry; holds no gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "Fragment_", menuName = "GmtkCountdown/Fragment Data")]
    public class FragmentData : ScriptableObject
    {
        [SerializeField] private string fragmentText;
        [SerializeField] private FragmentCategory category;
        [SerializeField] private int credibilityValue;

        /// <summary>The excuse fragment text (localized content filled in later).</summary>
        public string Text => fragmentText;

        /// <summary>The thematic category this fragment belongs to.</summary>
        public FragmentCategory Category => category;

        /// <summary>How much credibility this fragment contributes when used.</summary>
        public int CredibilityValue => credibilityValue;
    }
}
