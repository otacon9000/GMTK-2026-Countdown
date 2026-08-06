using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GmtkCountdown
{
    /// <summary>
    /// One card slot in the player's hand. Displays whatever fragment GameplayController puts in
    /// it and turns clicks into the two hand actions: left click plays the fragment, right click
    /// discards it. The slot holds no state of its own — <see cref="slotIndex"/> is its position
    /// in the hand, set in the Inspector, and everything else is pushed in by
    /// <see cref="RefreshDisplay"/>.
    /// </summary>
    public class CardSlotUI : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color EmptySlotColor = new Color(0.6f, 0.6f, 0.6f);

        // Deliberately loud and unlike any category colour: a category added to FragmentCategory
        // without a colour here compiles fine and would otherwise be indistinguishable from an
        // empty slot. Seeing magenta means "this category has no colour yet", not "no card".
        private static readonly Color UnknownCategoryColor = new Color(1f, 0f, 1f);

        [SerializeField] private int slotIndex;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private GameplayController controller;
        [SerializeField] private Image borderImage;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                controller.TryPlaySlot(slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                controller.TryDiscardSlot(slotIndex);
            }
        }

        /// <summary>
        /// Shows <paramref name="fragment"/>, or the empty-slot state when it is null. Called by
        /// GameplayController whenever the hand changes; this component never asks by itself.
        /// </summary>
        public void RefreshDisplay(FragmentData fragment)
        {
            labelText.text = fragment != null ? fragment.Text : "(empty)";
            borderImage.color = fragment != null ? GetCategoryColor(fragment.Category) : EmptySlotColor;
        }

        private static Color GetCategoryColor(FragmentCategory category)
        {
            return category switch
            {
                FragmentCategory.Technology => new Color(0.25f, 0.55f, 0.85f),
                FragmentCategory.Health => new Color(0.35f, 0.7f, 0.4f),
                FragmentCategory.Family => new Color(0.9f, 0.55f, 0.25f),
                FragmentCategory.ForceMajeure => new Color(0.55f, 0.35f, 0.75f),
                FragmentCategory.Absurd => new Color(0.9f, 0.35f, 0.65f),
                _ => UnknownCategoryColor
            };
        }
    }
}
