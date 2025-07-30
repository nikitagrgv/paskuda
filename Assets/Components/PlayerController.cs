using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Components
{
    [RequireComponent(typeof(GeneralCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;

        public float moveSpeedSprint = 9f;

        [Header("Control")]
        public float lookSensitivity = 0.06f;

        [Header("Crosshair")]
        public float crosshairSize = 20f;

        public float crosshairThickness = 2f;
        public Color crosshairColor = new(100, 50, 20, 100);

        private Vector2 _moveInputDir = Vector2.zero;

        private bool _isSprinting;

        private Texture2D _crosshairTexture;

        private GeneralCharacterController _ctrl;

        private void Start()
        {
            _ctrl = GetComponent<GeneralCharacterController>();

            _crosshairTexture = new Texture2D(1, 1);
            _crosshairTexture.SetPixel(0, 0, crosshairColor);
            _crosshairTexture.Apply();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInputDir = context.ReadValue<Vector2>();
            UpdateTargetVelocity();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 lookDir = context.ReadValue<Vector2>();
            _ctrl.LookPitch = _ctrl.LookPitch - lookDir.y * lookSensitivity;
            _ctrl.LookYaw = _ctrl.LookYaw + lookDir.x * lookSensitivity;
            UpdateTargetVelocity();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
            UpdateTargetVelocity();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                _ctrl.JumpRequest = GeneralCharacterController.JumpRequestType.TryJumpNow;
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                _ctrl.FireRequest = GeneralCharacterController.FireRequestType.TryFireNow;
            }
        }

        private void OnGUI()
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            GUI.DrawTexture(new Rect(screenCenter.x - crosshairSize / 2f, screenCenter.y - crosshairThickness / 2f,
                crosshairSize, crosshairThickness), _crosshairTexture);
            GUI.DrawTexture(new Rect(screenCenter.x - crosshairThickness / 2f, screenCenter.y - crosshairSize / 2f,
                crosshairThickness, crosshairSize), _crosshairTexture);
        }

        private void UpdateTargetVelocity()
        {
            if (_moveInputDir == Vector2.zero)
            {
                _ctrl.TargetVelocity = Vector3.zero;
                return;
            }

            Quaternion yawRotation = Quaternion.Euler(0, _ctrl.LookYaw, 0);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 dir = forward * _moveInputDir.y + right * _moveInputDir.x;
            dir.Normalize();
            float speed = _isSprinting ? moveSpeedSprint : moveSpeed;
            _ctrl.TargetVelocity = speed * dir;
        }
    }
}