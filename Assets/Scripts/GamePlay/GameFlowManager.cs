using Score;
using UnityEngine;

namespace GamePlay
{
    public class GameFlowManager : SingletonBaseClass<GameFlowManager>
    {
        [Header("UI")] 
        public UiGameOver gameOverUI;

        #region Setter and Getter

        public bool IsGameOver { get; private set; }

        #endregion

        private void Start()
        {
            IsGameOver = false;
        }
    
        /// <summary>
        /// Game over
        /// </summary>
        public void GameOver()
        {
            IsGameOver = true;
            ScoreManager.Instance.SetHighScore();
            gameOverUI.Show();
        }
    }
}
