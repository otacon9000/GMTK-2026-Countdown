using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Drives the automatic Countdown timer: while the game is in <see cref="GameState.Countdown"/>,
    /// counts down a fixed duration and transitions to <see cref="GameState.Interruption"/> when it
    /// reaches zero. No visual representation here — that's future work once real assets exist.
    /// </summary>
    public class CountdownController : MonoBehaviour
    {
        [SerializeField] private float countdownDuration = 8f;

        private float currentTime;

        /// <summary>Seconds remaining in the current countdown.</summary>
        public float TimeRemaining => currentTime;

        /// <summary>Remaining time as a 0-1 fraction of <see cref="countdownDuration"/>.</summary>
        public float TimeRemainingNormalized => countdownDuration > 0f ? currentTime / countdownDuration : 0f;

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
                currentTime = countdownDuration;
            }
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                GameManager.Instance.ChangeState(GameState.Interruption);
            }
        }
    }
}
