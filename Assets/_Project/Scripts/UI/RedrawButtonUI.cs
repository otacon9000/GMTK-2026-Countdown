using UnityEngine;
using UnityEngine.UI;

namespace GmtkCountdown
{
    public class RedrawButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameplayController controller;

        private void Awake()
        {
            button.onClick.AddListener(() => controller.TryRedraw());
        }

        /// <summary>
        /// Shows or hides the button according to whether a redraw is currently possible.
        /// Driven by GameplayController after a hand change or a state transition — this component
        /// deliberately has no Update of its own and never polls for the answer.
        /// </summary>
        public void RefreshVisibility()
        {
            gameObject.SetActive(controller.CanRedraw());
        }
    }
}
