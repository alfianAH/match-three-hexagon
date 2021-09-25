using UnityEngine;

namespace GamePlay
{
    public class TimeManager : SingletonBaseClass<TimeManager>
    {
        public int duration;

        private GameFlowManager gameFlowManager;
        private float time;

        private void Awake()
        {
            gameFlowManager = GameFlowManager.Instance;
        }

        private void Start()
        {
            time = 0;
        }

        private void Update()
        {
            if(gameFlowManager.IsGameOver) return;

            if (time > duration)
            {
                gameFlowManager.GameOver();
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
}
