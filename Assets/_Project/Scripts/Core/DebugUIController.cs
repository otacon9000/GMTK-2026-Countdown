using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Throwaway debug harness to manually drive the Countdown -> Interruption ->
    /// ComboResolution loop before real Countdown timer and card-prefab UI exist.
    /// Input and rendering here are intentionally OnGUI/keyboard only; replace this
    /// entirely once real UI is ready.
    /// </summary>
    public class DebugUIController : MonoBehaviour
    {
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private PromptData testPromptData;
        [SerializeField] private CountdownController countdownController;
        [SerializeField] private TaskManager taskManager;

        private List<FragmentData> currentHand = new List<FragmentData>();

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
                currentHand = deckManager.DrawHand(5);

                if (promptText != null && testPromptData != null)
                {
                    promptText.text = testPromptData.PromptText;
                }
            }

            // TaskCompleted stub: bounce straight back to Countdown, no celebration screen yet.
            if (newState == GameState.TaskCompleted)
            {
                Debug.Log($"[DebugUIController] Task completed, starting next task (Task {taskManager.CurrentTaskIndex + 1})");
                GameManager.Instance.ChangeState(GameState.Countdown);
            }
        }

        private void Update()
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameState.Countdown:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        GameManager.Instance.ChangeState(GameState.Interruption);
                    }
                    break;

                case GameState.Interruption:
                    HandleFragmentSelectionInput();
                    break;
            }
        }

        private void HandleFragmentSelectionInput()
        {
            for (int i = 0; i < 5; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(key))
                {
                    SelectFragment(i);
                    break;
                }
            }
        }

        private void SelectFragment(int index)
        {
            if (index >= currentHand.Count)
            {
                return;
            }

            FragmentData fragment = currentHand[index];
            deckManager.RemoveFragment(fragment);
            int effectiveValue = taskManager.PlayFragment(fragment);

            Debug.Log($"[DebugUIController] Picked '{fragment.Text}' ({fragment.Category}, effective {effectiveValue}) - accumulated credibility: {taskManager.AccumulatedCredibility}");

            taskManager.EvaluateProgress(deckManager);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 300));
            GUILayout.Label($"State: {GameManager.Instance.CurrentState}");
            GUILayout.Label($"Fragments remaining in deck: {deckManager.RemainingCount}");
            GUILayout.Label($"Task {taskManager.CurrentTaskIndex + 1} - threshold: {taskManager.CurrentThreshold}");
            GUILayout.Label($"Accumulated credibility: {taskManager.AccumulatedCredibility}");

            if (GameManager.Instance.CurrentState == GameState.Countdown && countdownController != null)
            {
                GUILayout.Label($"Countdown: {countdownController.TimeRemaining:F1}s");
            }

            if (GameManager.Instance.CurrentState == GameState.Interruption)
            {
                for (int i = 0; i < currentHand.Count; i++)
                {
                    GUILayout.Label($"{i + 1}: {currentHand[i].Text}");
                }
            }

            if (GameManager.Instance.CurrentState == GameState.GameOver)
            {
                GUILayout.Label($"GAME OVER - Tasks completed: {taskManager.CurrentTaskIndex}");
            }

            GUILayout.Space(10);
            GUILayout.Label("SPACE = trigger interruption (Countdown only) | 1-5 = pick fragment (Interruption only)");
            GUILayout.EndArea();
        }
    }
}
