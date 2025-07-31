using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class UIOverlay : MonoBehaviour
    {
        public Health health;
        public Image healthImage;

        private void Start()
        {
            health.HealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged()
        {
            healthImage.fillAmount = health.CurrentHealthPercentage;
        }
    }
}