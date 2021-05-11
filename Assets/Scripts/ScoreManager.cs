using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static int highScore;

    public int tileRatio;
    public int comboRatio;

    #region Singleton

    private static ScoreManager instance;

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScoreManager>();

                if (instance == null)
                {
                    Debug.LogError("Fatal Error: ScoreManager not Found");
                }
            }

            return instance;
        }
    }

    #endregion

    #region Setter and Getter

    public int HighScore => highScore;
    public int CurrentScore { get; private set; }

    #endregion

    private void Start()
    {
        ResetCurrentScore();
    }
    
    /// <summary>
    /// Reset current score to 0
    /// </summary>
    public void ResetCurrentScore()
    {
        CurrentScore = 0;
    }
    
    /// <summary>
    /// Increase current score
    /// </summary>
    /// <param name="tileCount">Amount of destroyed tile</param>
    /// <param name="comboCount">Amount of combo</param>
    public void IncrementCurrentScore(int tileCount, int comboCount)
    {
        CurrentScore += (tileCount * tileRatio) * (comboCount * comboRatio);
        
        // Play sound when combo is more than 1
        // SoundManager.Instance.PlayScore(comboCount > 1);
    }
    
    /// <summary>
    /// Set high score
    /// </summary>
    public void SetHighScore()
    {
        highScore = CurrentScore;
    }
    
}
