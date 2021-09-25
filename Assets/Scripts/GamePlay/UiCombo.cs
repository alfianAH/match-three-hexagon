using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class UiCombo: MonoBehaviour
    {
        [SerializeField] private Text comboText;
        
        /// <summary>
        /// Update combo text
        /// </summary>
        /// <param name="comboValue">Current combo value</param>
        public void UpdateComboText(int comboValue)
        {
            comboText.text = $"Combo x{comboValue}";
        }
    }
}