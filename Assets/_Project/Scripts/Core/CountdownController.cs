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
        [SerializeField] private float countdownDuration = 4f;
        [SerializeField] private float minimumCountdownDuration = 2f;

        // The three durations, in the order they flow: queued for the next round, running for
        // the current one, and how much of it is left.
        private float? queuedDuration;
        private float activeDuration;
        private float timeRemaining;

        /// <summary>Remaining time as a 0-1 fraction of the current countdown's starting duration.</summary>
        public float TimeRemainingNormalized => activeDuration > 0f ? timeRemaining / activeDuration : 0f;

        /// <summary>The starting duration (seconds) of the current/most recent Countdown round.</summary>
        public float ActiveDuration => activeDuration;

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
            queuedDuration = Mathf.Max(earnedTime, minimumCountdownDuration);
        }

        /// <summary>
        /// Spends <paramref name="amount"/> seconds of the current countdown time, e.g. as the
        /// cost of an in-Countdown-state action like drawing a new fragment. Clamped at 0; does
        /// not trigger the Interruption transition, which is handled by Update().
        /// </summary>
        public void ConsumeTime(float amount)
        {
            timeRemaining = Mathf.Max(0f, timeRemaining - amount);
        }

        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.Countdown)
            {
                if (queuedDuration.HasValue)
                {
                    activeDuration = queuedDuration.Value;
                    queuedDuration = null;
                }
                else
                {
                    activeDuration = countdownDuration;
                }

                timeRemaining = activeDuration;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                GameManager.Instance.ChangeState(GameState.Interruption);
            }
        }
    }
}
