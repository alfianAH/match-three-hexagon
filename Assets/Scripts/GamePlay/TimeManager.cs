using UnityEngine;

namespace GamePlay
{
    public class TimeManager : SingletonBaseClass<TimeManager>
    {
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
}
