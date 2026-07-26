using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GmtkCountdown
{
    public class CardSlotUI : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color EmptySlotColor = new Color(0.6f, 0.6f, 0.6f);

        [SerializeField] private int slotIndex;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private DebugUIController controller;
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
                _ => EmptySlotColor
            };
        }
    }
}
