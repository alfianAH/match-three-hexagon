namespace GamePlay
{
    public class ComboManager: SingletonBaseClass<ComboManager>
    {
        public UiCombo uiCombo;
        
        /// <summary>
        /// Show game object
        /// </summary>
        public void Show()
        {
            uiCombo.gameObject.SetActive(true);
        }
    
        /// <summary>
        /// Hide game object
        /// </summary>
        public void Hide()
        {
            uiCombo.gameObject.SetActive(false);
        }
    }
}