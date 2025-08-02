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
        public float runMoveSprint = 9f;

        public float walkMoveSpeed = 5f;

        [Header("Control")]
        public float lookSensitivity = 0.06f;

        private Vector2 _moveInputDir = Vector2.zero;

        private bool _isWalking;

        private GeneralCharacterController _ctrl;

        private void Start()
        {
            _ctrl = GetComponent<GeneralCharacterController>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInputDir = context.ReadValue<Vector2>();
            UpdateTargetVelocity();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 lookDir = context.ReadValue<Vector2>();
            _ctrl.LookPitch -= lookDir.y * lookSensitivity;
            _ctrl.LookYaw += lookDir.x * lookSensitivity;
            UpdateTargetVelocity();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector3 dir = InputDirToGlobalDir(_moveInputDir, _ctrl.LookYaw);
                _ctrl.RequestDash(dir, GeneralCharacterController.ActionRequestType.TryNow);
            }
        }

        public void OnToggleSlowMotion(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            if (Mathf.Approximately(Time.timeScale, 1f))
            {
                Time.timeScale = 0.1f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _ctrl.JumpRequest = GeneralCharacterController.ActionRequestType.TryNow;
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _ctrl.FireRequest = GeneralCharacterController.ActionRequestType.DoRepeat;
            }
            else if (context.canceled)
            {
                _ctrl.FireRequest = GeneralCharacterController.ActionRequestType.NotRequested;
            }
        }

        private void UpdateTargetVelocity()
        {
            if (_moveInputDir.sqrMagnitude <= 0.0001f)
            {
                _ctrl.TargetVelocity = Vector3.zero;
                return;
            }

            Vector3 dir = InputDirToGlobalDir(_moveInputDir, _ctrl.LookYaw);
            float speed = _isWalking ? walkMoveSpeed : runMoveSprint;
            _ctrl.TargetVelocity = speed * dir;
        }

        private static Vector3 InputDirToGlobalDir(Vector2 dir, float yaw)
        {
            Quaternion yawRotation = Quaternion.Euler(0, yaw, 0);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 globDir = forward * dir.y + right * dir.x;
            globDir.Normalize();
            return globDir;
        }
    }
}