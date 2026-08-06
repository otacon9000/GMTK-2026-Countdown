using UnityEngine;

namespace GmtkCountdown
{
    /// <summary>
    /// Moves the boss between two points based on CountdownController's timer, and drives
    /// sprite flipping and walk animation. First Countdown is a straight walk-in from
    /// pointA to pointB; every subsequent Countdown retreats partway back toward pointA
    /// and returns, with retreat distance scaled by the round's duration.
    /// </summary>
    public class BossMover : MonoBehaviour
    {
        // Fraction of a Countdown round the boss spends retreating away from pointB.
        // The remainder of the round is spent walking back to pointB, so that the boss
        // is always at pointB exactly when the countdown hits zero.
        private const float RetreatPhaseFraction = 0.5f;

        // Horizontal movement below this is treated as standing still and does not flip
        // the sprite, so float jitter at the turnaround point can't cause a flicker.
        private const float FlipMovementThreshold = 0.001f;

        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private CountdownController countdownController;
        [SerializeField] private Animator animator;
        [SerializeField] private float distancePerSecond = 1.5f;

        // False until the boss has finished its one-off walk-in from pointA to pointB,
        // which ends at the first Interruption. From then on every Countdown is a
        // retreat-and-return cycle instead.
        private bool hasCompletedWalkIn = false;
        private float originalScaleX;
        private Vector3 lastPosition;

        private void Awake()
        {
            // Every reference below is dereferenced every frame in Update, so a missing one
            // would mean an exception per frame. Report all of them at once — fixing them one
            // Play session at a time is the slow way — and switch the component off so the
            // error is reported once instead of drowning the console.
            bool missingReference = ReportIfMissing(pointA, nameof(pointA));
            missingReference |= ReportIfMissing(pointB, nameof(pointB));
            missingReference |= ReportIfMissing(countdownController, nameof(countdownController));
            missingReference |= ReportIfMissing(animator, nameof(animator));

            if (missingReference)
            {
                enabled = false;
            }
        }

        private bool ReportIfMissing(UnityEngine.Object reference, string fieldName)
        {
            if (reference != null)
            {
                return false;
            }

            Debug.LogError($"[BossMover] '{fieldName}' is not assigned in the Inspector; disabling this component.", this);
            return true;
        }

        private void Start()
        {
            transform.position = pointA.position;
            originalScaleX = Mathf.Abs(transform.localScale.x);
            lastPosition = transform.position;
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
            if (newState == GameState.Interruption && !hasCompletedWalkIn)
            {
                hasCompletedWalkIn = true;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            animator.SetBool(IsWalkingHash, GameManager.Instance.CurrentState == GameState.Countdown);

            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            float normalized = countdownController.TimeRemainingNormalized;
            Vector3 newPosition;

            if (!hasCompletedWalkIn)
            {
                newPosition = Vector3.Lerp(pointA.position, pointB.position, 1f - normalized);
            }
            else
            {
                float p = 1f - normalized;
                float roundDuration = countdownController.ActiveDuration;
                float retreatDistance = Mathf.Min(roundDuration * distancePerSecond, Vector3.Distance(pointB.position, pointA.position));
                Vector3 retreatPoint = Vector3.MoveTowards(pointB.position, pointA.position, retreatDistance);

                if (p < RetreatPhaseFraction)
                {
                    float t = p / RetreatPhaseFraction;
                    newPosition = Vector3.Lerp(pointB.position, retreatPoint, t);
                }
                else
                {
                    float t = (p - RetreatPhaseFraction) / (1f - RetreatPhaseFraction);
                    newPosition = Vector3.Lerp(retreatPoint, pointB.position, t);
                }
            }

            if (newPosition.x > lastPosition.x + FlipMovementThreshold)
            {
                transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
            }
            else if (newPosition.x < lastPosition.x - FlipMovementThreshold)
            {
                transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
            }

            transform.position = newPosition;
            lastPosition = transform.position;
        }
    }
}
