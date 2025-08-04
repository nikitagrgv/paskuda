using UnityEngine;
using UnityEngine.UI;

namespace Code.Components
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
            if (!_cam)
            {
                _cam = Camera.main;
                if (!_cam)
                {
                    return;
                }
            }

            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }

        private void OnHealthChanged(Health.HealthChangeInfo info)
        {
            healthImage.fillAmount = health.CurrentHealthPercentage;
        }
    }
}