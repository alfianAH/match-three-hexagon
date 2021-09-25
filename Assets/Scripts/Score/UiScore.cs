using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    public class UiScore : MonoBehaviour
    {
        public Text highScore;
        public Text currentScore;

        private void Update()
        {
            highScore.text = ScoreManager.Instance.HighScore.ToString();
            currentScore.text = ScoreManager.Instance.CurrentScore.ToString();
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
