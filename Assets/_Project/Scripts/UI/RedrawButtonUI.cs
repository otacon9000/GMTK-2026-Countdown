using UnityEngine;
using UnityEngine.UI;

namespace GmtkCountdown
{
    public class RedrawButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private DebugUIController controller;

        private void Awake()
        {
            button.onClick.AddListener(() => controller.TryRedraw());
        }

        public void RefreshVisibility()
        {
            gameObject.SetActive(controller.CanRedraw());
        }
    }
}
