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
            _ctrl.LookPitch = _ctrl.LookPitch - lookDir.y * lookSensitivity;
            _ctrl.LookYaw = _ctrl.LookYaw + lookDir.x * lookSensitivity;
            UpdateTargetVelocity();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                _ctrl.DashRequest = GeneralCharacterController.ActionRequestType.TryNow;
            }
        }

        public void OnWalk(InputAction.CallbackContext context)
        {
            _isWalking = context.ReadValueAsButton();
            UpdateTargetVelocity();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                _ctrl.JumpRequest = GeneralCharacterController.ActionRequestType.TryNow;
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                _ctrl.FireRequest = GeneralCharacterController.ActionRequestType.TryNow;
            }
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
            float speed = _isWalking ? walkMoveSpeed : runMoveSprint;
            _ctrl.TargetVelocity = speed * dir;
        }
    }
}