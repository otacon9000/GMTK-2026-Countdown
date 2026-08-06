using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Main menu screen shown at game start. Freezes gameplay via Time.timeScale until the
    /// player presses Play, and covers the screen with its own panel while it is up.
    /// <para>
    /// It deliberately does not touch the gameplay UI groups: GameplayController is their single
    /// owner and drives them from state transitions. This class used to keep a second copy of
    /// that list and switch it off in Awake, but the switch-off never had any visible effect —
    /// GameManager fires the first Countdown from Start(), and GameplayController turns the
    /// groups back on before anything is ever drawn. What actually hides gameplay behind this
    /// screen is the menu panel itself, raised to the top of the canvas below.
    /// </para>
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanelRoot;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Animator[] animatorsToKeepRunning;

        private void Awake()
        {
            Time.timeScale = 0f;

            if (menuPanelRoot != null)
            {
                menuPanelRoot.SetActive(true);
                menuPanelRoot.transform.SetAsLastSibling();
            }

            if (menuAnimator != null)
            {
                // Menu animations must keep playing while gameplay is frozen.
                menuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            SetAnimatorsToKeepRunningUpdateMode(AnimatorUpdateMode.UnscaledTime);
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
    }
}
