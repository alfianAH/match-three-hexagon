using Score;
using UnityEngine;

namespace GamePlay
{
    public class GameFlowManager : SingletonBaseClass<GameFlowManager>
    {
        [Header("UI")] 
        public UiGameOver gameOverUI;

        private BoardManager boardManager;

        #region Setter and Getter

        public bool IsGameOver { get; private set; }

        #endregion

        private void Awake()
        {
            boardManager = BoardManager.Instance;
        }

        private void Start()
        {
            IsGameOver = false;
        }
    
        /// <summary>
        /// Game over
        /// </summary>
        public void GameOver()
        {
            if(!boardManager.IsAnimating)
            {
                IsGameOver = true;
                ScoreManager.Instance.SetHighScore();
                gameOverUI.Show();
            }
        }
    }
}
