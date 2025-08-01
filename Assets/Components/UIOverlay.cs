using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class UIOverlay : MonoBehaviour
    {
        public GeneralCharacterController playerController;
        public Health health;
        public Image healthImage;
        public Image dashImage;
        public Image reloadProgressImage;

        private readonly Color _norReadyColor = new(1f, 0.5f, 0.5f);
        private readonly Color _readyColor = new(0.5f, 1f, 0.5f);

        private bool _oldDashReady;

        private void Start()
        {
            health.HealthChanged += OnHealthChanged;
            UpdateDash(playerController.IsDashReady);
        }

        private void LateUpdate()
        {
            bool dashReady = playerController.IsDashReady;
            if (dashReady != _oldDashReady)
            {
                UpdateDash(dashReady);
            }

            float reload = playerController.RemainingReloadTimeNormalized;
            reloadProgressImage.fillAmount = reload;
        }

        private void OnHealthChanged()
        {
            healthImage.fillAmount = health.CurrentHealthPercentage;
        }

        private void UpdateDash(bool ready)
        {
            dashImage.color = ready ? _readyColor : _norReadyColor;
            _oldDashReady = ready;
        }
    }
}