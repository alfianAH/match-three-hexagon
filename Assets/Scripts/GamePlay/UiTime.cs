using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class UiTime : MonoBehaviour
    {
        public Text time;

        private void Update()
        {
            time.text = GetTimeString(TimeManager.Instance.GetRemainingTime() + 1);
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
    
        /// <summary>
        /// Convert time to string
        /// </summary>
        /// <param name="timeRemaining">Time remaining</param>
        /// <returns>"{minute} : {second}"</returns>
        private string GetTimeString(float timeRemaining)
        {
            int minute = Mathf.FloorToInt(timeRemaining / 60);
            int second = Mathf.FloorToInt(timeRemaining % 60);

            return $"{minute:00} : {second:00}";
        }
    }
}
