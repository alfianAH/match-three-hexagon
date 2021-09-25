using UnityEngine;

namespace GamePlay
{
    public class TimeManager : SingletonBaseClass<TimeManager>
    {
        public int duration;
        
        [SerializeField] private Animator timeAnimator;
        
        private GameFlowManager gameFlowManager;
        private float time;
        private static readonly int StopAnim = Animator.StringToHash("stopAnim");
        private static readonly int PlayAnim = Animator.StringToHash("playAnim");

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
                timeAnimator.SetTrigger(StopAnim);
                gameFlowManager.GameOver();
                return;
            }

            time += Time.deltaTime;

            if (GetRemainingTime() < 10.0f)
            {
                timeAnimator.SetTrigger(PlayAnim);
            }
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
