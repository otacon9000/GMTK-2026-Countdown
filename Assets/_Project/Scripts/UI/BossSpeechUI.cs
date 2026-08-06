using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// The boss's speech bubble. Shows a random line for the duration of an Interruption and
    /// clears itself in every other state, so the bubble is on screen exactly while the boss is
    /// waiting for an answer.
    /// </summary>
    public class BossSpeechUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text speechText;
        [SerializeField] private GameObject speechAreaRoot;

        // Content lives in assets, like prompts, fragments and tasks: adding a line is authoring
        // work, not a code edit. An empty pool is not fatal — the bubble just stays quiet.
        [SerializeField] private List<BossLineData> bossLines = new List<BossLineData>();

        // Hiding the speech area happens in Awake rather than Start, for the same reason as in
        // GameplayController: GameManager fires the first transition from Start(), and the order
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
                speechText.text = PickRandomLine();
                speechAreaRoot.SetActive(true);
            }
            else
            {
                speechText.text = string.Empty;
                speechAreaRoot.SetActive(false);
            }
        }

        private string PickRandomLine()
        {
            if (bossLines == null || bossLines.Count == 0)
            {
                Debug.LogWarning("[BossSpeechUI] Boss line pool is empty; the boss has nothing to say.", this);
                return string.Empty;
            }

            BossLineData line = bossLines[Random.Range(0, bossLines.Count)];
            return line != null ? line.Text : string.Empty;
        }
    }
}
