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
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private CountdownController countdownController;
        [SerializeField] private Animator animator;
        [SerializeField] private float distancePerSecond = 1.5f;

        private bool hasEverStarted = false;
        private float originalScaleX;
        private Vector3 lastPosition;

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
            if (newState == GameState.Interruption && !hasEverStarted)
            {
                hasEverStarted = true;
            }
        }

        private void Update()
        {
            animator.SetBool("IsWalking", GameManager.Instance.CurrentState == GameState.Countdown);

            if (GameManager.Instance.CurrentState != GameState.Countdown)
            {
                return;
            }

            float normalized = countdownController.TimeRemainingNormalized;
            Vector3 newPosition;

            if (!hasEverStarted)
            {
                newPosition = Vector3.Lerp(pointA.position, pointB.position, 1f - normalized);
            }
            else
            {
                float p = 1f - normalized;
                float currentDuration = countdownController.CurrentActiveDuration;
                float retreatDistance = Mathf.Min(currentDuration * distancePerSecond, Vector3.Distance(pointB.position, pointA.position));
                Vector3 retreatPoint = Vector3.MoveTowards(pointB.position, pointA.position, retreatDistance);

                if (p < 0.5f)
                {
                    float t = p / 0.5f;
                    newPosition = Vector3.Lerp(pointB.position, retreatPoint, t);
                }
                else
                {
                    float t = (p - 0.5f) / 0.5f;
                    newPosition = Vector3.Lerp(retreatPoint, pointB.position, t);
                }
            }

            if (newPosition.x > lastPosition.x + 0.001f)
            {
                transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
            }
            else if (newPosition.x < lastPosition.x - 0.001f)
            {
                transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
            }

            transform.position = newPosition;
            lastPosition = transform.position;
        }
    }
}
