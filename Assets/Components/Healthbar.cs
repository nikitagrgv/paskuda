using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class Healthbar : MonoBehaviour
    {
        public Health health;
        public Image healthImage;

        private Camera _cam;

        private bool _noCamera = false;

        private void Start()
        {
            _cam = Camera.main;
            health.HealthChanged += OnHealthChanged;
        }

        private void LateUpdate()
        {
            if (_noCamera)
            {
                return;
            }

            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null)
                {
                    _noCamera = true;
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