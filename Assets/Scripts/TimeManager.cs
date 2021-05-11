using UnityEngine;

public class TimeManager : MonoBehaviour
{
    #region Singleton

    private static TimeManager instance;

    public static TimeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TimeManager>();
                
                if (instance == null)
                {
                    Debug.LogError("Fatal Error: TimeManager not Found");
                }
            }

            return instance;
        }
    }

    #endregion

    public int duration;

    private float time;

    private void Start()
    {
        time = 0;
    }

    private void Update()
    {
        if(GameFlowManager.Instance.IsGameOver) return;

        if (time > duration)
        {
            GameFlowManager.Instance.GameOver();
            return;
        }

        time += Time.deltaTime;
    }
    
    /// <summary>
    /// Get remaining time
    /// </summary>
    /// <returns>Duration - Time</returns>
    public float GetRemainingTime()
    {
        return duration - time;
    }
}
