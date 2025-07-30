using UnityEngine;

namespace Components
{
    public class BotController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;

        public float moveSpeedSprint = 9f;

        public float minPeriodUpdateYaw = 0.1f;
        public float maxPeriodUpdateYaw = 4f;
        public float speedRotateYaw = 200f;

        public float minPeriodUpdatePitch = 0.1f;
        public float maxPeriodUpdatePitch = 4f;
        public float speedRotatePitch = 200f;

        public float minPeriodJump = 0.1f;
        public float maxPeriodJump = 5f;

        public float minPeriodChangeWantedPosition = 0.1f;
        public float maxPeriodChangeWantedPosition = 20f;
        public float wantedPositionMaxRadius = 50f;

        private GeneralCharacterController _ctrl;

        private float _timerUpdateYaw;
        private float _targetYaw;

        private float _timerUpdatePitch;
        private float _targetPitch;

        private float _timerJump;

        private float _timerChangeWantedPosition;
        private Vector3 _wantedPosition;

        private void Start()
        {
            _ctrl = GetComponent<GeneralCharacterController>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            UpdateRotation(dt);
            UpdateJump(dt);
            UpdateTargetPosition(dt);
        }

        private void UpdateRotation(float dt)
        {
            // Yaw
            _timerUpdateYaw -= dt;
            if (_timerUpdateYaw <= 0)
            {
                _timerUpdateYaw = Random.Range(minPeriodUpdateYaw, maxPeriodUpdateYaw);
                _targetYaw = Random.Range(0, 360);
            }

            float totalDeltaYaw = _targetYaw - _ctrl.LookYaw;
            if (totalDeltaYaw > 180) totalDeltaYaw -= 360;
            if (totalDeltaYaw < -180) totalDeltaYaw += 360;

            float deltaYawSign = Mathf.Sign(totalDeltaYaw);
            float deltaYaw = deltaYawSign * speedRotateYaw * dt;
            if (Mathf.Abs(deltaYaw) > Mathf.Abs(totalDeltaYaw))
            {
                _ctrl.LookYaw = _targetYaw;
            }
            else
            {
                _ctrl.LookYaw += deltaYaw;
            }

            // Pitch
            _timerUpdatePitch -= dt;
            if (_timerUpdatePitch <= 0)
            {
                _timerUpdatePitch = Random.Range(minPeriodUpdatePitch, maxPeriodUpdatePitch);
                _targetPitch = Random.Range(GeneralCharacterController.MinPitch, GeneralCharacterController.MaxPitch);
            }

            float totalDeltaPitch = _targetPitch - _ctrl.LookPitch;
            float deltaPitchSign = Mathf.Sign(totalDeltaPitch);
            float deltaPitch = deltaPitchSign * speedRotatePitch * dt;
            if (Mathf.Abs(deltaPitch) > Mathf.Abs(totalDeltaPitch))
            {
                _ctrl.LookPitch = _targetPitch;
            }
            else
            {
                _ctrl.LookPitch += deltaPitch;
            }
        }

        private void UpdateJump(float dt)
        {
            _timerJump -= dt;
            if (_timerJump <= 0)
            {
                _timerJump = Random.Range(minPeriodJump, maxPeriodJump);
                _ctrl.JumpRequest = GeneralCharacterController.JumpRequestType.TryJumpNow;
            }
        }

        private void UpdateTargetPosition(float dt)
        {
            _timerChangeWantedPosition -= dt;
            if (_timerChangeWantedPosition <= 0)
            {
                _timerChangeWantedPosition = Random.Range(minPeriodChangeWantedPosition, maxPeriodChangeWantedPosition);
                Vector2 rand = Random.insideUnitCircle * wantedPositionMaxRadius;
                _wantedPosition = new Vector3(rand.x, 0, rand.y);
            }

            Vector3 curPosition = transform.position;
            Vector3 deltaPosition = _wantedPosition - curPosition;
            deltaPosition.y = 0;
            float deltaMagnitude = deltaPosition.magnitude;
            if (deltaMagnitude < 0.5f)
            {
                _ctrl.TargetVelocity = Vector3.zero;
            }
            else
            {
                _ctrl.TargetVelocity = deltaPosition.normalized * moveSpeed;
            }
        }
    }
}