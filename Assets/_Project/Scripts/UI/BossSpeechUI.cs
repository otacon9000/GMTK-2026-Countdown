using TMPro;
using UnityEngine;

namespace GmtkCountdown
{
    public class BossSpeechUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text speechText;
        [SerializeField] private GameObject speechAreaRoot;
        [SerializeField] private string[] bossLines = new string[]
        {
            "Are you done yet?",
            "Well?",
            "I'm waiting."
        };

        private void Start()
        {
            speechAreaRoot.SetActive(false);
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
            if (newState == GameState.Interruption)
            {
                speechText.text = bossLines[Random.Range(0, bossLines.Length)];
                speechAreaRoot.SetActive(true);
            }
            else
            {
                speechText.text = "";
                speechAreaRoot.SetActive(false);
            }
        }
    }
}
