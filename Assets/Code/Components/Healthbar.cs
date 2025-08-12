using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Components
{
    public class Healthbar : MonoBehaviour
    {
        public Health health;
        public Image healthImage;

        private CameraManager _cameraManager;

        [Inject]
        public void Construct(CameraManager cameraManager)
        {
            _cameraManager = cameraManager;
        }

        private void Start()
        {
            health.HealthChanged += OnHealthChanged;
        }

        private void LateUpdate()
        {
            Camera cam = _cameraManager.ActiveCamera;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }

        private void OnHealthChanged(Health.HealthChangeInfo info)
        {
            healthImage.fillAmount = health.CurrentHealthPercentage;
        }
    }
}