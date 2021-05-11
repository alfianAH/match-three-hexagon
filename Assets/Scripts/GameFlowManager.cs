using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    #region Singleton

    private static GameFlowManager instance;

    public static GameFlowManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameFlowManager>();
                
                if (instance == null)
                {
                    Debug.LogError("Fatal Error: GameFlowManager not Found");
                }
            }

            return instance;
        }
    }

    #endregion

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
