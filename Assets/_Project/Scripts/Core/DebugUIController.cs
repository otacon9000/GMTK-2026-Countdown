using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Main gameplay controller, despite the name: it owns the player's hand, drives the
    /// Countdown -> Interruption -> Break loop in response to <see cref="GameManager.OnStateChanged"/>,
    /// and toggles the gameplay UI groups. The hand is persistent across a task — fragments stay
    /// in it until discarded or played, and are refilled for free only at the start of a run and
    /// after each completed task; any other refill costs countdown time via TryRedraw.
    /// Both mouse (through CardSlotUI / RedrawButtonUI / TaskChoiceButtonUI) and keyboard drive
    /// the same public Try* methods, which are the entry points for every player action.
    /// The "Debug" in the name is a leftover from the jam; a rename is pending.
    /// </summary>
    public class DebugUIController : MonoBehaviour
    {
        private const int HandCapacity = 4;
        private const float RedrawCost = 3f;

        [SerializeField] private DeckManager deckManager;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private List<PromptData> promptPool;
        [SerializeField] private CountdownController countdownController;
        [SerializeField] private TaskManager taskManager;
        [SerializeField] private CardSlotUI[] cardSlots;
        [SerializeField] private RedrawButtonUI redrawButton;
        [SerializeField] private TaskChoiceButtonUI[] taskChoiceButtons;
        [SerializeField] private GameObject breakChoicePanel;
        [SerializeField] private GameOverPanelUI gameOverPanel;
        [SerializeField] private GameObject gameOverPanelRoot;
        [SerializeField] private GameObject[] gameplayUIRoots;
        [SerializeField] private GameObject cardHandAreaRoot;
        [SerializeField] private GameObject promptAreaRoot;

        private const int TaskChoiceCount = 3;

        // One entry per hand position, all starting empty. HandCapacity is the single source of
        // truth for how big the hand is: the cardSlots array is only the view onto it, and may
        // legitimately have a different length if the scene is mid-edit.
        private readonly List<FragmentData> hand = new List<FragmentData>(new FragmentData[HandCapacity]);
        private List<TaskData> currentTaskChoices = new List<TaskData>();

        // True right before a Countdown state that should get a free full refill:
        // the very first Countdown of a run, and every TaskCompleted -> Countdown bounce.
        private bool freeRefillPending = true;

        // Panel visibility is set up in Awake rather than Start on purpose: GameManager fires
        // the run's first state transition from its own Start(), and Unity does not define the
        // order of Start() between components. Every Awake() is guaranteed to run before any
        // Start(), so these panels are always hidden before the first transition arrives.
        private void Awake()
        {
            if (breakChoicePanel != null)
            {
                breakChoicePanel.SetActive(false);
            }

            if (gameOverPanelRoot != null)
            {
                gameOverPanelRoot.SetActive(false);
            }

            if (promptAreaRoot != null)
            {
                promptAreaRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
            GameManager.HasPlayableFragment = HasPlayableFragmentInHand;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;

            if (GameManager.HasPlayableFragment == (Func<bool>)HasPlayableFragmentInHand)
            {
                GameManager.HasPlayableFragment = null;
            }
        }

        private bool HasPlayableFragmentInHand()
        {
            foreach (FragmentData fragment in hand)
            {
                if (fragment != null)
                {
                    return true;
                }
            }

            return false;
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

        private void HandleStateChanged(GameState newState)
        {
            if (promptAreaRoot != null)
            {
                if (newState == GameState.Interruption)
                {
                    promptAreaRoot.SetActive(true);
                }
                else
                {
                    promptAreaRoot.SetActive(false);
                }
            }

            if (gameOverPanelRoot != null)
            {
                gameOverPanelRoot.SetActive(newState == GameState.GameOver);
            }

            if (newState == GameState.GameOver)
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.RefreshDisplay(taskManager.CurrentTaskIndex, taskManager.TotalScore);
                }

                SetGameplayUIRootsActive(false);
            }

            if (newState == GameState.Countdown)
            {
                SetGameplayUIRootsActive(true);

                if (freeRefillPending)
                {
                    RefillHandFree();
                    freeRefillPending = false;
                }

                if (promptText != null)
                {
                    promptText.text = string.Empty;
                }

                if (breakChoicePanel != null)
                {
                    breakChoicePanel.SetActive(false);
                }
            }

            if (newState == GameState.Interruption)
            {
                if (cardHandAreaRoot != null)
                {
                    cardHandAreaRoot.SetActive(true);
                }

                if (promptText != null)
                {
                    if (promptPool == null || promptPool.Count == 0)
                    {
                        Debug.LogWarning("[DebugUIController] Prompt pool is empty, skipping prompt text update");
                    }
                    else
                    {
                        PromptData chosenPrompt = promptPool[UnityEngine.Random.Range(0, promptPool.Count)];
                        promptText.text = chosenPrompt.BuildSentence("_____");
                    }
                }
            }

            // TaskCompleted stub: bounce straight to the Break task-selection screen.
            if (newState == GameState.TaskCompleted)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DebugUIController] Task completed, moving to Break (Task {taskManager.CurrentTaskIndex + 1})");
#endif
                freeRefillPending = true;
                GameManager.Instance.ChangeState(GameState.Break);
            }

            if (newState == GameState.Break)
            {
                currentTaskChoices = taskManager.GetTaskChoices(TaskChoiceCount);

                if (breakChoicePanel != null)
                {
                    breakChoicePanel.SetActive(true);
                }

                if (taskChoiceButtons != null)
                {
                    for (int i = 0; i < taskChoiceButtons.Length; i++)
                    {
                        if (taskChoiceButtons[i] != null)
                        {
                            taskChoiceButtons[i].RefreshDisplay(GetTaskChoice(i));
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            switch (GameManager.Instance.CurrentState)
            {
                case GameState.Countdown:
                    HandleCountdownInput();
                    break;

                case GameState.Interruption:
                    HandleInterruptionInput();
                    break;

                case GameState.Break:
                    HandleBreakInput();
                    break;
            }

            RefreshHandUI();
        }

        private void RefreshHandUI()
        {
            if (cardSlots != null)
            {
                for (int i = 0; i < cardSlots.Length; i++)
                {
                    if (cardSlots[i] != null)
                    {
                        cardSlots[i].RefreshDisplay(GetHandSlot(i));
                    }
                }
            }

            if (redrawButton != null)
            {
                redrawButton.RefreshVisibility();
            }
        }

        private void HandleBreakInput()
        {
            for (int i = 0; i < TaskChoiceCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TrySelectTaskChoice(i);
                    break;
                }
            }
        }

        private void HandleCountdownInput()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Shortcut to skip the countdown wait while testing. Compiled only into the
            // editor and development builds, so external playtest builds keep it while the
            // public build cannot: there it would let the game be played with no time
            // pressure at all.
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.ChangeState(GameState.Interruption);
            }
#endif

            for (int i = 0; i < HandCapacity; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryDiscardSlot(i);
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryRedraw();
            }
        }

        private void HandleInterruptionInput()
        {
            for (int i = 0; i < HandCapacity; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryPlaySlot(i);
                    break;
                }
            }
        }

        public void TryDiscardSlot(int index)
        {
            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            if (index < 0 || index >= hand.Count)
            {
                return;
            }

            if (hand[index] == null)
            {
                return;
            }

            int filledSlots = 0;
            foreach (FragmentData fragment in hand)
            {
                if (fragment != null)
                {
                    filledSlots++;
                }
            }

            if (filledSlots <= 1)
            {
                // Discarding the last card in hand would soft-lock the next Interruption; block it.
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DebugUIController] Discarded '{hand[index].Text}' from slot {index + 1}");
#endif
            hand[index] = null;
        }

        public void TryRedraw()
        {
            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DebugUIController] Redrew '{drawn[0].Text}' into slot {emptyIndex + 1} (-{RedrawCost}s)");
#endif
        }

        public void TryPlaySlot(int index)
        {
            if (GameManager.Instance.CurrentState != GameState.Interruption)
            {
                return;
            }

            if (index < 0 || index >= hand.Count)
            {
                return;
            }

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DebugUIController] Picked '{fragment.Text}' ({fragment.Category}, effective {effectiveValue}) - accumulated credibility: {taskManager.AccumulatedCredibility} -> next countdown: {effectiveValue}s");
#endif

            taskManager.EvaluateProgress(deckManager);
        }

        public void TrySelectTaskChoice(int index)
        {
            if (GameManager.Instance.CurrentState != GameState.Break)
            {
                return;
            }

            if (index < 0 || index >= currentTaskChoices.Count)
            {
                return;
            }

            taskManager.SelectTask(currentTaskChoices[index]);
        }

        public TaskData GetTaskChoice(int index)
        {
            if (index < 0 || index >= currentTaskChoices.Count)
            {
                return null;
            }

            return currentTaskChoices[index];
        }

        public bool HasEmptySlot()
        {
            return hand.FindIndex(fragment => fragment == null) >= 0;
        }

        public bool CanRedraw()
        {
            return GameManager.Instance.CurrentState == GameState.Countdown && HasEmptySlot() && !deckManager.IsEmpty;
        }

        public FragmentData GetHandSlot(int index)
        {
            if (index < 0 || index >= hand.Count)
            {
                return null;
            }

            return hand[index];
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
    }
}
