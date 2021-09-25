using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay
{
    public class UiGameOver : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
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
