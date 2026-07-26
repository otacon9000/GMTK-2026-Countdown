using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GmtkCountdown
{
    public class GameOverPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text tasksCompletedText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button retryButton;

        private void Awake()
        {
            retryButton.onClick.AddListener(RetryGame);
        }

        public void RefreshDisplay(int tasksCompleted, int totalScore)
        {
            tasksCompletedText.text = $"Tasks Completed: {tasksCompleted}";
            scoreText.text = $"Score: {totalScore}";
        }

        private void RetryGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
