using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    public class BossSpeechUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text speechText;
        [SerializeField] private GameObject speechAreaRoot;
        [SerializeField] private string[] bossLines = new string[]
        {
            "Are you done yet?",
            "Well?",
            "I'm waiting."
        };

        // Hiding the speech area happens in Awake rather than Start, for the same reason as in
        // DebugUIController: GameManager fires the first transition from Start(), and the order
        // of Start() between components is undefined.
        private void Awake()
        {
            bool missingReference = ReportIfMissing(speechText, nameof(speechText));
            missingReference |= ReportIfMissing(speechAreaRoot, nameof(speechAreaRoot));

            if (missingReference)
            {
                // Disabling before OnEnable also means this never subscribes to OnStateChanged,
                // so the missing reference can't blow up on the first Interruption either.
                enabled = false;
                return;
            }

            speechAreaRoot.SetActive(false);
        }

        private bool ReportIfMissing(UnityEngine.Object reference, string fieldName)
        {
            if (reference != null)
            {
                return false;
            }

            Debug.LogError($"[BossSpeechUI] '{fieldName}' is not assigned in the Inspector; disabling this component.", this);
            return true;
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.Interruption)
            {
                speechText.text = bossLines[Random.Range(0, bossLines.Length)];
                speechAreaRoot.SetActive(true);
            }
            else
            {
                speechText.text = "";
                speechAreaRoot.SetActive(false);
            }
        }
    }
}
