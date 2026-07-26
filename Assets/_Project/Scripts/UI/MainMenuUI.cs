using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Main menu screen shown at game start. Freezes gameplay via Time.timeScale until
    /// the player presses Play, and keeps all gameplay UI hidden until then.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanelRoot;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject[] gameplayUIRoots;
        [SerializeField] private Animator[] animatorsToKeepRunning;

        private void Awake()
        {
            Time.timeScale = 0f;

            if (menuAnimator != null)
            {
                // Menu animations must keep playing while gameplay is frozen.
                menuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            SetAnimatorsToKeepRunningUpdateMode(AnimatorUpdateMode.UnscaledTime);

            SetGameplayUIRootsActive(false);
        }

        public void OnPlayClicked()
        {
            Time.timeScale = 1f;

            if (menuAnimator != null)
            {
                menuAnimator.updateMode = AnimatorUpdateMode.Normal;
            }

            SetAnimatorsToKeepRunningUpdateMode(AnimatorUpdateMode.Normal);

            if (menuPanelRoot != null)
            {
                menuPanelRoot.SetActive(false);
            }

            SetGameplayUIRootsActive(true);
        }

        private void SetAnimatorsToKeepRunningUpdateMode(AnimatorUpdateMode updateMode)
        {
            if (animatorsToKeepRunning == null)
            {
                return;
            }

            foreach (Animator animator in animatorsToKeepRunning)
            {
                if (animator != null)
                {
                    animator.updateMode = updateMode;
                }
            }
        }

        private void SetGameplayUIRootsActive(bool active)
        {
            if (gameplayUIRoots == null)
            {
                return;
            }

            foreach (GameObject root in gameplayUIRoots)
            {
                if (root != null)
                {
                    root.SetActive(active);
                }
            }
        }
    }
}
