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

        private GameState currentState = GameState.Countdown;

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
        }

        private void Start()
        {
            ChangeState(GameState.Countdown);
        }

        /// <summary>
        /// Transitions the game to <paramref name="newState"/>, updates the current state and
        /// notifies subscribers. Keep this a flat switch: no per-state Enter/Exit methods for now.
        /// </summary>
        /// <param name="newState">The state to transition into.</param>
        public void ChangeState(GameState newState)
        {
            switch (newState)
            {
                case GameState.Countdown:
                    break;
                case GameState.Interruption:
                    break;
                case GameState.ComboResolution:
                    break;
                case GameState.TaskCompleted:
                    break;
                case GameState.GameOver:
                    break;
                case GameState.Break:
                    break;
            }

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            OnStateChanged?.Invoke(newState);
        }
    }
}
