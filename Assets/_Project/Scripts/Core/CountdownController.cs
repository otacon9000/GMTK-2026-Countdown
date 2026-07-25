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
        [SerializeField] private float minimumCountdownDuration = 2f;

        private float currentTime;
        private float currentActiveDuration;
        private float? nextCountdownDuration;

        /// <summary>Seconds remaining in the current countdown.</summary>
        public float TimeRemaining => currentTime;

        /// <summary>Remaining time as a 0-1 fraction of the current countdown's starting duration.</summary>
        public float TimeRemainingNormalized => currentActiveDuration > 0f ? currentTime / currentActiveDuration : 0f;

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        /// <summary>
        /// Queues the duration for the next Countdown, based on the effective time value
        /// earned from the fragment just played. Clamped to <see cref="minimumCountdownDuration"/>.
        /// </summary>
        public void SetNextCountdownDuration(float earnedTime)
        {
            nextCountdownDuration = Mathf.Max(earnedTime, minimumCountdownDuration);
        }

        /// <summary>
        /// Spends <paramref name="amount"/> seconds of the current countdown time, e.g. as the
        /// cost of an in-Countdown-state action like drawing a new fragment. Clamped at 0; does
        /// not trigger the Interruption transition, which is handled by Update().
        /// </summary>
        public void ConsumeTime(float amount)
        {
            currentTime = Mathf.Max(0f, currentTime - amount);
        }

        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.Countdown)
            {
                if (nextCountdownDuration.HasValue)
                {
                    currentActiveDuration = nextCountdownDuration.Value;
                    nextCountdownDuration = null;
                }
                else
                {
                    currentActiveDuration = countdownDuration;
                }

                currentTime = currentActiveDuration;
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
