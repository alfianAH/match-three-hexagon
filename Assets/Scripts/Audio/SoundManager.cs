using UnityEngine;

namespace Audio
{
    public class SoundManager : SingletonBaseClass<SoundManager>
    {
        public AudioClip scoreNormal;
        public AudioClip scoreCombo;

        public AudioClip wrongMove;
    
        public AudioClip tap;

        private AudioSource player;

        private void Start()
        {
            player = GetComponent<AudioSource>();
        }
    
        /// <summary>
        /// Play score audio
        /// </summary>
        /// <param name="isCombo">Is combo more than 1?</param>
        public void PlayScore(bool isCombo)
        {
            player.PlayOneShot(isCombo 
                ? scoreCombo 
                : scoreNormal);
        }
    
        /// <summary>
        /// Play tap audio when player tap the tile
        /// </summary>
        public void PlayTap()
        {
            player.PlayOneShot(tap);
        }
    
        /// <summary>
        /// Play wrong audio when player is wrong
        /// </summary>
        public void PlayWrong()
        {
            player.PlayOneShot(wrongMove);
        }
    }
}
