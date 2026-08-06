using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GmtkCountdown
{
    public class TaskChoiceButtonUI : MonoBehaviour
    {
        [SerializeField] private int choiceIndex;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private GameplayController controller;

        private void Awake()
        {
            button.onClick.AddListener(() => controller.TrySelectTaskChoice(choiceIndex));
        }

        public void RefreshDisplay(TaskData task)
        {
            if (task != null)
            {
                nameText.text = task.Name;
                detailsText.text = $"Time: {task.RequiredTime}s | Score: {task.ScoreValue}";
            }
            else
            {
                nameText.text = "(no option)";
                detailsText.text = string.Empty;
            }
        }
    }
}
