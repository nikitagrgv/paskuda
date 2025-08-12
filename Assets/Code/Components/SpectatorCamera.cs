using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Components
{
    public class SpectatorCamera : MonoBehaviour
    {
        public Camera spectatorCamera;

        public float normalSpeed = 10f;
        public float fastSpeed = 30f;

        public float mouseSensitivity = 0.06f;
        public float gamepadSensitivity = 60f;
        public float gamepadFastSensitivity = 160f;

        public float LookPitch
        {
            get => _lookPitch;
            set => _lookPitch = Mathf.Clamp(value, -89f, 89f);
        }

        public float LookYaw
        {
            get => _lookYaw;
            set => _lookYaw = Utils.ToAngleFrom0To360(value);
        }

        private bool _fastSpeedEnabled;
        private bool _gamepadFastLook;

        private Vector2 _moveInputDir = Vector2.zero;
        private float _verticalDir;

        private float _lookPitch;
        private float _lookYaw;

        private Vector2 _lookGamepadInputDir = Vector2.zero;

        private Vector3 _velocity;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (_lookGamepadInputDir != Vector2.zero)
            {
                float sensitivity = _gamepadFastLook ? gamepadFastSensitivity : gamepadSensitivity;
                float mul = sensitivity * dt;
                LookPitch -= _lookGamepadInputDir.y * mul;
                LookYaw += _lookGamepadInputDir.x * mul;
                UpdateVelocity();
            }

            Quaternion rot = Quaternion.Euler(LookPitch, LookYaw, 0.0f);
            transform.rotation = rot;

            transform.position += _velocity * dt;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _moveInputDir = Vector2.zero;
                UpdateVelocity();
                return;
            }

            if (!context.performed)
                return;

            _moveInputDir = context.ReadValue<Vector2>();
            UpdateVelocity();
        }

        public void OnMoveUpAndDown(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _verticalDir = 0;
                UpdateVelocity();
                return;
            }

            if (!context.performed)
                return;

            _verticalDir = context.ReadValue<float>();
            UpdateVelocity();
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
            LookPitch -= lookDir.y * mul;
            LookYaw += lookDir.x * mul;
            UpdateVelocity();
        }

        public void OnFastSpeed(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _fastSpeedEnabled = false;
                UpdateVelocity();
                return;
            }

            if (context.performed)
            {
                _fastSpeedEnabled = true;
                UpdateVelocity();
            }
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

        private void UpdateVelocity()
        {
            const float eps = 0.0001f;
            if (_moveInputDir.sqrMagnitude <= eps && Mathf.Abs(_verticalDir) <= eps)
            {
                _velocity = Vector3.zero;
                return;
            }

            Vector3 dir = InputToGlobalVector(_moveInputDir, _verticalDir, LookPitch, LookYaw);
            float speed = _fastSpeedEnabled ? fastSpeed : normalSpeed;
            _velocity = speed * dir;
        }

        private static Vector3 InputToGlobalVector(Vector2 dir, float verticalDir, float pitch, float yaw)
        {
            Quaternion yawRotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 up = yawRotation * Vector3.up;
            Vector3 globDir = forward * dir.y + right * dir.x + up * verticalDir;
            return globDir;
        }
    }
}