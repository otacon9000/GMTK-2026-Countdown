using System;
using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Central state machine for the game session. Holds the current <see cref="GameState"/>,
    /// exposes transitions through <see cref="ChangeState"/>, and notifies other systems via
    /// <see cref="OnStateChanged"/>. This class is intentionally free of gameplay logic:
    /// systems such as DeckManager, UIController and CountdownController subscribe to the event
    /// and react independently.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Global access point to the single GameManager present in the scene.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Raised after every successful state transition, carrying the new state.
        /// Systems subscribe to this to react to state changes without coupling to GameManager.
        /// </summary>
        public static event Action<GameState> OnStateChanged;

        /// <summary>
        /// Consulted before entering <see cref="GameState.Interruption"/>: an Interruption with
        /// nothing in hand has no move in it, so the request is redirected to
        /// <see cref="GameState.GameOver"/> instead. This is the one gameplay question this class
        /// asks, and it is wired in the Inspector rather than registered at runtime, so there is
        /// exactly one answerer and it is visible without reading any other file.
        /// </summary>
        [SerializeField] private GameplayController gameplayController;

        private GameState currentState = GameState.Countdown;

        // True while OnStateChanged is being raised. A subscriber is allowed to request another
        // transition from inside its handler; queueing it here rather than running it immediately
        // is what keeps CurrentState in step with the newState every subscriber is being handed.
        private bool isNotifying;

        // The transition a subscriber asked for while the current one was still being delivered.
        // A single slot, not a queue: only one such request exists today (TaskCompleted -> Break),
        // and if two subscribers ever asked during the same notification the last one would win.
        private GameState? pendingState;

        /// <summary>
        /// The state the game is currently in. Read-only from outside; change it via
        /// <see cref="ChangeState"/>.
        /// </summary>
        public GameState CurrentState => currentState;

        private void Awake()
        {
            // Single-scene jam scope: enforce one instance, no DontDestroyOnLoad.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (gameplayController == null)
            {
                // Fail open rather than closed: without an answerer the Interruption goes ahead,
                // which is what happened before this was an explicit reference. It can leave the
                // game with nothing to do in Interruption, so the error has to be loud.
                Debug.LogError($"[GameManager] '{nameof(gameplayController)}' is not assigned in the Inspector; an Interruption with an empty hand can no longer be redirected to GameOver.", this);
            }
        }

        private void Start()
        {
            ChangeState(GameState.Countdown);
        }

        /// <summary>
        /// Transitions the game to <paramref name="newState"/>, updates the current state and
        /// notifies subscribers. There is deliberately no per-state logic here: everything that
        /// has to happen on a given state belongs in a subscriber to <see cref="OnStateChanged"/>.
        /// <para>
        /// Calling this from inside a subscriber is allowed and is how TaskCompleted moves on to
        /// Break. Such a call does not transition immediately: it is queued and applied once the
        /// current notification has reached every subscriber. Without that, subscribers later in
        /// the invocation list would still be receiving the old state as
        /// <paramref name="newState"/> while <see cref="CurrentState"/> already held the new one,
        /// and would receive the two transitions in reverse order — a discrepancy that depends on
        /// subscription order and would surface as an unreproducible bug.
        /// </para>
        /// </summary>
        /// <param name="newState">The state to transition into.</param>
        public void ChangeState(GameState newState)
        {
            if (newState == GameState.Interruption && gameplayController != null && !gameplayController.HasPlayableFragment())
            {
                newState = GameState.GameOver;
            }

            if (isNotifying)
            {
                pendingState = newState;
                return;
            }

            isNotifying = true;

            try
            {
                GameState stateToApply = newState;

                while (true)
                {
                    currentState = stateToApply;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[GameManager] State changed to: {stateToApply}");
#endif
                    OnStateChanged?.Invoke(stateToApply);

                    if (!pendingState.HasValue)
                    {
                        break;
                    }

                    stateToApply = pendingState.Value;
                    pendingState = null;
                }
            }
            finally
            {
                // Without this, a subscriber throwing would leave the flag set and every later
                // transition would be queued and never applied: the game would freeze silently.
                isNotifying = false;
            }
        }
    }
}
