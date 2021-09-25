using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    public class UiScore : MonoBehaviour
    {
        public Text highScore;
        public Text currentScore;

        private ScoreManager scoreManager;

        private void Awake()
        {
            scoreManager = ScoreManager.Instance;
        }

        private void Update()
        {
            highScore.text = scoreManager.HighScore.ToString();
            currentScore.text = scoreManager.CurrentScore.ToString();
        }

        /// <summary>
        /// Show game object
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }
    
        /// <summary>
        /// Hide game object
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
