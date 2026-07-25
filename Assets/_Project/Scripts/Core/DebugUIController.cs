using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Throwaway debug harness to manually drive the Countdown -> Interruption ->
    /// ComboResolution loop before real Countdown timer and card-prefab UI exist.
    /// Hand is persistent across a task: fragments are drawn one at a time via
    /// DeckManager.DrawFragments and stay in the hand until discarded or played.
    /// Input and rendering here are intentionally OnGUI/keyboard only; replace this
    /// entirely once real UI is ready.
    /// </summary>
    public class DebugUIController : MonoBehaviour
    {
        private const int HandCapacity = 4;
        private const float RedrawCost = 3f;

        [SerializeField] private DeckManager deckManager;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private PromptData testPromptData;
        [SerializeField] private CountdownController countdownController;
        [SerializeField] private TaskManager taskManager;

        private readonly List<FragmentData> hand = new List<FragmentData> { null, null, null, null };
        private int? lastQueuedCountdownDuration;

        // True right before a Countdown state that should get a free full refill:
        // the very first Countdown of a run, and every TaskCompleted -> Countdown bounce.
        private bool freeRefillPending = true;

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
            if (newState == GameState.Countdown)
            {
                if (freeRefillPending)
                {
                    RefillHandFree();
                    freeRefillPending = false;
                }
            }

            if (newState == GameState.Interruption)
            {
                if (promptText != null && testPromptData != null)
                {
                    promptText.text = testPromptData.PromptText;
                }
            }

            // TaskCompleted stub: bounce straight back to Countdown, no celebration screen yet.
            if (newState == GameState.TaskCompleted)
            {
                Debug.Log($"[DebugUIController] Task completed, starting next task (Task {taskManager.CurrentTaskIndex + 1})");
                freeRefillPending = true;
                GameManager.Instance.ChangeState(GameState.Countdown);
            }
        }

        private void Update()
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameState.Countdown:
                    HandleCountdownInput();
                    break;

                case GameState.Interruption:
                    HandleInterruptionInput();
                    break;
            }
        }

        private void HandleCountdownInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.ChangeState(GameState.Interruption);
            }

            for (int i = 0; i < HandCapacity; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    DiscardFragment(i);
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RedrawFragment();
            }
        }

        private void HandleInterruptionInput()
        {
            for (int i = 0; i < HandCapacity; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    PlayFragment(i);
                    break;
                }
            }
        }

        private void DiscardFragment(int index)
        {
            if (hand[index] == null)
            {
                return;
            }

            Debug.Log($"[DebugUIController] Discarded '{hand[index].Text}' from slot {index + 1}");
            hand[index] = null;
        }

        private void RedrawFragment()
        {
            int emptyIndex = hand.FindIndex(fragment => fragment == null);
            if (emptyIndex < 0 || deckManager.IsEmpty)
            {
                return;
            }

            List<FragmentData> drawn = deckManager.DrawFragments(1);
            if (drawn.Count == 0)
            {
                return;
            }

            hand[emptyIndex] = drawn[0];
            countdownController.ConsumeTime(RedrawCost);

            Debug.Log($"[DebugUIController] Redrew '{drawn[0].Text}' into slot {emptyIndex + 1} (-{RedrawCost}s)");
        }

        private void PlayFragment(int index)
        {
            FragmentData fragment = hand[index];
            if (fragment == null)
            {
                return;
            }

            hand[index] = null;
            int effectiveValue = taskManager.PlayFragment(fragment);

            if (countdownController != null)
            {
                countdownController.SetNextCountdownDuration(effectiveValue);
            }

            lastQueuedCountdownDuration = effectiveValue;

            Debug.Log($"[DebugUIController] Picked '{fragment.Text}' ({fragment.Category}, effective {effectiveValue}) - accumulated credibility: {taskManager.AccumulatedCredibility} -> next countdown: {effectiveValue}s");

            taskManager.EvaluateProgress(deckManager);
        }

        private void RefillHandFree()
        {
            int emptySlots = 0;
            foreach (FragmentData fragment in hand)
            {
                if (fragment == null)
                {
                    emptySlots++;
                }
            }

            if (emptySlots <= 0)
            {
                return;
            }

            List<FragmentData> drawn = deckManager.DrawFragments(emptySlots);

            int drawnIndex = 0;
            for (int i = 0; i < hand.Count && drawnIndex < drawn.Count; i++)
            {
                if (hand[i] == null)
                {
                    hand[i] = drawn[drawnIndex];
                    drawnIndex++;
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 400));
            GUILayout.Label($"State: {GameManager.Instance.CurrentState}");
            GUILayout.Label($"Fragments remaining in deck: {deckManager.RemainingCount}");
            GUILayout.Label($"Task {taskManager.CurrentTaskIndex + 1} - threshold: {taskManager.CurrentThreshold}");
            GUILayout.Label($"Accumulated credibility: {taskManager.AccumulatedCredibility}");

            if (lastQueuedCountdownDuration.HasValue)
            {
                GUILayout.Label($"Next countdown queued: {lastQueuedCountdownDuration.Value}s");
            }

            if (GameManager.Instance.CurrentState == GameState.Countdown && countdownController != null)
            {
                GUILayout.Label($"Countdown: {countdownController.TimeRemaining:F1}s");
            }

            GUILayout.Space(10);
            GUILayout.Label("Hand:");
            for (int i = 0; i < HandCapacity; i++)
            {
                FragmentData fragment = hand[i];
                string label = fragment != null ? $"{fragment.Text} ({fragment.Category})" : "(empty)";
                GUILayout.Label($"{i + 1}: {label}");
            }

            if (GameManager.Instance.CurrentState == GameState.GameOver)
            {
                GUILayout.Label($"GAME OVER - Tasks completed: {taskManager.CurrentTaskIndex}");
            }

            GUILayout.Space(10);
            GUILayout.Label("SPACE = trigger interruption (Countdown only)");
            GUILayout.Label("Countdown: 1-4 discard | R = redraw (-3s) | Interruption: 1-4 = play fragment");
            GUILayout.EndArea();
        }
    }
}
