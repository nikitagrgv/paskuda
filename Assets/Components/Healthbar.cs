using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class Healthbar : MonoBehaviour
    {
        public Health health;
        public Image healthImage;

        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            health.HealthChanged += OnHealthChanged;
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.LookRotation(_cam.transform.position - transform.position);
        }

        private void OnHealthChanged()
        {
            healthImage.fillAmount = health.CurrentHealthPercentage;
        }
    }
}