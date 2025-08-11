using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Code.Components
{
    [RequireComponent(typeof(GeneralCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float runMoveSprint = 9f;

        public float walkMoveSpeed = 5f;

        [Header("Control")]
        public float mouseSensitivity = 0.06f;

        public float gamepadSensitivity = 60f;
        public float gamepadFastSensitivity = 160f;

        private Vector2 _moveInputDir = Vector2.zero;

        private Vector2 _lookGamepadInputDir = Vector2.zero;
        private bool _gamepadFastLook;

        private bool _isWalking;

        private GeneralCharacterController _ctrl;

        private TimeController _timeController;

        [Inject]
        private void Construct(TimeController timeController)
        {
            _timeController = timeController;
        }

        private void Start()
        {
            _ctrl = GetComponent<GeneralCharacterController>();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_lookGamepadInputDir != Vector2.zero)
            {
                float sensitivity = _gamepadFastLook ? gamepadFastSensitivity : gamepadSensitivity;
                float mul = sensitivity * dt;
                _ctrl.LookPitch -= _lookGamepadInputDir.y * mul;
                _ctrl.LookYaw += _lookGamepadInputDir.x * mul;
                UpdateTargetVelocity();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _moveInputDir = Vector2.zero;
                UpdateTargetVelocity();
                return;
            }

            if (!context.performed)
                return;

            _moveInputDir = context.ReadValue<Vector2>();
            UpdateTargetVelocity();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _lookGamepadInputDir = Vector2.zero;
                return;
            }

            if (!context.performed)
            {
                return;
            }

            bool gamepad = context.action.activeControl?.device is Gamepad;
            Vector2 lookDir = context.ReadValue<Vector2>();
            if (gamepad)
            {
                _lookGamepadInputDir = lookDir;
                return;
            }

            float mul = mouseSensitivity;

            _lookGamepadInputDir = Vector2.zero;
            _ctrl.LookPitch -= lookDir.y * mul;
            _ctrl.LookYaw += lookDir.x * mul;
            UpdateTargetVelocity();
        }

        public void OnGamepadFastLook(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _gamepadFastLook = false;
            }

            if (context.performed)
            {
                _gamepadFastLook = true;
            }
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
            
            Debug.Log(context);

            if (_timeController.IsTimeScaleChanged())
            {
                _timeController.ResetTimeScale();
            }
            else
            {
                _timeController.SetTimeScale(0.25f);
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

            Vector3 dir = InputToGlobalVector(_moveInputDir, _ctrl.LookYaw);
            float speed = _isWalking ? walkMoveSpeed : runMoveSprint;
            _ctrl.TargetVelocity = speed * dir;
        }

        private static Vector3 InputDirToGlobalDir(Vector2 dir, float yaw)
        {
            return InputToGlobalVector(dir, yaw).normalized;
        }

        private static Vector3 InputToGlobalVector(Vector2 dir, float yaw)
        {
            Quaternion yawRotation = Quaternion.Euler(0, yaw, 0);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 globDir = forward * dir.y + right * dir.x;
            return globDir;
        }
    }
}