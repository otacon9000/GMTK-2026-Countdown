using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Main gameplay controller: it owns the player's hand, drives the
    /// Countdown -> Interruption -> Break loop in response to <see cref="GameManager.OnStateChanged"/>,
    /// and toggles the gameplay UI groups. The hand is persistent across a task — fragments stay
    /// in it until discarded or played, and are refilled for free only at the start of a run and
    /// after each completed task; any other refill costs countdown time via TryRedraw.
    /// Both mouse (through CardSlotUI / RedrawButtonUI / TaskChoiceButtonUI) and keyboard drive
    /// the same public Try* methods, which are the entry points for every player action.
    /// </summary>
    public class GameplayController : MonoBehaviour
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

        // HandCapacity is the single source of truth for how big the hand is: the cardSlots array
        // is only the view onto it, and may legitimately have a different length if the scene is
        // mid-edit. Every rule about what is in the hand lives in Hand; this class only decides
        // when those rules may run and what they cost.
        private readonly Hand hand = new Hand(HandCapacity);
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
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        // ---------------------------------------------------------------------------------------
        // When a run ends. Both questions live here, side by side, because this is the only class
        // that can see both the hand and the deck. They are deliberately different questions, and
        // the difference is the whole rule:
        //
        //   HasPlayableFragment  - can the player act *right now*?
        //   CanContinueRun       - can the player act *at all*, now or after the coming Countdown?
        //
        // A full deck is worth nothing at the moment an Interruption starts, because drawing is
        // only possible during a Countdown; but it is worth everything right after a play, because
        // a Countdown is exactly what comes next.
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// True when the player holds at least one fragment they could play. Asked by GameManager
        /// before entering an Interruption: with nothing in hand there is no move to make, and the
        /// run is over however many fragments are left in the deck.
        /// </summary>
        public bool HasPlayableFragment()
        {
            return hand.HasAnyFragment;
        }

        /// <summary>
        /// True when the run can go on: the player either still holds a fragment, or can still
        /// redraw one during the Countdown that is about to start. Asked after a fragment has been
        /// played and the current task is still unfinished. An empty hand is not the end as long
        /// as the deck can refill it — that Countdown is the player's last chance, and spending it
        /// without redrawing is what actually ends the run.
        /// </summary>
        public bool CanContinueRun()
        {
            return hand.HasAnyFragment || !deckManager.IsEmpty;
        }

        /// <summary>
        /// Switches the gameplay UI groups on or off. This class is their single owner — no other
        /// component may touch <see cref="gameplayUIRoots"/>, so that "when is gameplay UI visible"
        /// has exactly one answer, driven by state transitions.
        /// </summary>
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
                    gameOverPanel.RefreshDisplay(taskManager.TasksCompleted, taskManager.TotalScore);
                }

                SetGameplayUIRootsActive(false);
            }

            if (newState == GameState.Countdown)
            {
                SetGameplayUIRootsActive(true);

                if (freeRefillPending)
                {
                    hand.RefillEmptySlots(deckManager);
                    freeRefillPending = false;
                    RefreshHandSlots();
                }

                if (promptText != null)
                {
                    promptText.text = string.Empty;
                }

                // Order matters here: breakChoicePanel is also an entry of gameplayUIRoots, so the
                // call above has just switched it back on and this is what puts it away again.
                // Moving this before SetGameplayUIRootsActive(true) would leave the Break panel
                // up for the whole Countdown.
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
                        Debug.LogWarning("[GameplayController] Prompt pool is empty, skipping prompt text update");
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
                Debug.Log($"[GameplayController] Task completed, moving to Break ({taskManager.TasksCompleted} completed so far)");
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

            // Last, so it sees the visibility the branches above have just applied: entering
            // Countdown re-enables the whole gameplayUIRoots group, redraw button included.
            RefreshRedrawButton();
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
        }

        /// <summary>
        /// Repaints everything that depends on the hand. Call right after any mutation of
        /// <see cref="hand"/>; nothing polls for these changes.
        /// </summary>
        private void RefreshHandUI()
        {
            RefreshHandSlots();
            RefreshRedrawButton();
        }

        /// <summary>
        /// Repaints the card slots. They show the hand and nothing else, so the hand changing is
        /// the only thing that can make them stale.
        /// </summary>
        private void RefreshHandSlots()
        {
            if (cardSlots == null)
            {
                return;
            }

            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (cardSlots[i] != null)
                {
                    cardSlots[i].RefreshDisplay(GetHandSlot(i));
                }
            }
        }

        /// <summary>
        /// Re-evaluates whether the redraw button should be on screen. Unlike the card slots this
        /// depends on three things — the current state, a free slot in hand, and fragments left in
        /// the deck — so it is refreshed both after a hand mutation and at the end of every state
        /// transition. The state transition case is not optional: the button's GameObject is one
        /// of the gameplayUIRoots, so entering Countdown switches it back on wholesale and this is
        /// what puts it away again when a redraw isn't actually possible.
        /// </summary>
        private void RefreshRedrawButton()
        {
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

            FragmentData discarded = hand.Discard(index);
            if (discarded == null)
            {
                return;
            }

            RefreshHandUI();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameplayController] Discarded '{discarded.Text}' from slot {index + 1}");
#endif
        }

        public void TryRedraw()
        {
            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            // Charge only once a fragment has actually landed: a -1 means the deck was never
            // touched, and the player must not pay countdown seconds for a draw that failed.
            int filledSlot = hand.TryDrawIntoEmptySlot(deckManager);
            if (filledSlot < 0)
            {
                return;
            }

            countdownController.ConsumeTime(RedrawCost);
            RefreshHandUI();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameplayController] Redrew '{hand.GetSlot(filledSlot).Text}' into slot {filledSlot + 1} (-{RedrawCost}s)");
#endif
        }

        public void TryPlaySlot(int index)
        {
            if (GameManager.Instance.CurrentState != GameState.Interruption)
            {
                return;
            }

            FragmentData fragment = hand.Play(index);
            if (fragment == null)
            {
                return;
            }

            RefreshHandUI();

            int earnedTime = taskManager.PlayFragment(fragment);

            if (countdownController != null)
            {
                countdownController.SetNextCountdownDuration(earnedTime);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[GameplayController] Picked '{fragment.Text}' ({fragment.Category}, earned {earnedTime}) - accumulated earned time: {taskManager.AccumulatedEarnedTime}/{taskManager.CurrentRequiredTime} -> next countdown: {earnedTime}s");
#endif

            // The single decision point for how a run ends: if the task isn't finished, the run
            // carries on only while the player still has something to play or to draw.
            if (!taskManager.TryCompleteCurrentTask())
            {
                GameManager.Instance.ChangeState(CanContinueRun() ? GameState.Countdown : GameState.GameOver);
            }
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
            return hand.HasEmptySlot;
        }

        public bool CanRedraw()
        {
            return GameManager.Instance.CurrentState == GameState.Countdown && HasEmptySlot() && !deckManager.IsEmpty;
        }

        public FragmentData GetHandSlot(int index)
        {
            return hand.GetSlot(index);
        }
    }
}
