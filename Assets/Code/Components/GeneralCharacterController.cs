using System;
using Code.Weapons;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Code.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class GeneralCharacterController : MonoBehaviour
    {
        public enum ActionRequestType
        {
            NotRequested,
            TryNow,
            DoWhenReadyAndFinish,
            DoRepeat,
        }

        public Health health;

        public GameObject eyeObject;
        public GameObject firePoint;

        [Header("Movement")]
        public float minPitch = -87f;

        public float maxPitch = 87f;

        public float moveAccelerationOnGround = 0.7f;

        public float moveAccelerationOnFly = 0.55f;
        public float jumpHeight = 2f;
        public float jumpReloadTime = 0.1f;
        public float maxSlopeAngle = 45f;

        public float dashVelocityChange = 20f;
        public float dashReloadTime = 2f;

        [Header("Ground Check")]
        public LayerMask groundLayers = Physics.AllLayers;

        public float groundCheckerRadius = 0.49f;

        public float groundCheckerOffset = 0.57f;
        public float normalCheckMaxDistance = 0.5f;

        [Header("Weapons")]
        [SerializeField]
        private WeaponMeta initialWeapon;

        public float LookPitch
        {
            get => _lookPitch;
            set => _lookPitch = Mathf.Clamp(value, minPitch, maxPitch);
        }

        public float LookYaw
        {
            get => _lookYaw;
            set => _lookYaw = Utils.ToAngleFrom0To360(value);
        }

        public ActionRequestType JumpRequest
        {
            get => _jumpRequest;
            set => _jumpRequest = value;
        }

        public ActionRequestType FireRequest
        {
            get => _fireRequest;
            set => _fireRequest = value;
        }

        public Vector3 TargetVelocity
        {
            get => _targetVelocity;
            set => _targetVelocity = value;
        }

        public void RequestDash(Vector3 dirXZ, ActionRequestType requestType)
        {
            _dashTargetXZ = dirXZ;
            _dashTargetXZ.y = 0;
            float magnitude = _dashTargetXZ.magnitude;
            if (magnitude <= 0.01)
            {
                CancelDashRequest();
                return;
            }

            _dashTargetXZ /= magnitude;
            _dashRequest = requestType;
        }

        public void CancelDashRequest()
        {
            _dashTargetXZ = Vector3.zero;
            _dashRequest = ActionRequestType.NotRequested;
        }

        public bool IsDashReady => _dashTimer <= 0;
        public float RemainingDashTimeNormalized => Mathf.Clamp(_dashTimer / dashReloadTime, 0f, 1f);

        public bool IsJumpReady => _jumpTimer <= 0 && _hasDoubleJump;

        public WeaponMeta ActiveWeapon
        {
            get => _activeWeapon.Meta;
            set
            {
                initialWeapon = value;
                _activeWeapon.Meta = value;
            }
        }

        public float RemainingFireTimeNormalized => _activeWeapon.RemainingTimeNormalized;
        public int AmmoInMagazine => _activeWeapon.AmmoInMagazine;
        public int TotalAmmo => _activeWeapon.TotalAmmo;
        public int AmmoNotInMagazine => _activeWeapon.AmmoNotInMagazine;

        public event Action Fired;

        private ProjectileManager _projectileManager;

        private Rigidbody _rb;
        private RelationshipsActor _relationshipsActor;

        private float _lookPitch;
        private float _lookYaw;

        private Vector3 _targetVelocity = Vector3.zero;
        private bool _isGrounded;
        private bool _isGoodAngle;

        private ActionRequestType _jumpRequest = ActionRequestType.NotRequested;
        private float _jumpTimer;
        private bool _hasDoubleJump;

        private ActionRequestType _fireRequest = ActionRequestType.NotRequested;
        private Weapon _activeWeapon = new();

        private Vector3 _dashTargetXZ = Vector3.zero;
        private ActionRequestType _dashRequest = ActionRequestType.NotRequested;
        private float _dashTimer;

        private readonly Collider[] _colliderBuffer = new Collider[10];
        private readonly RaycastHit[] _raycastHitBuffer = new RaycastHit[10];

        [Inject]
        public void Construct(ProjectileManager projectileManager)
        {
            _projectileManager = projectileManager;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            health.Died += OnDied;

            _relationshipsActor = GetComponent<RelationshipsActor>();
        }

        private void OnEnable()
        {
            _activeWeapon.Meta = initialWeapon;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            eyeObject.transform.rotation = Quaternion.Euler(_lookPitch, _lookYaw, 0);

            _activeWeapon.Update(dt);
            if (_fireRequest != ActionRequestType.NotRequested)
            {
                bool done = DoFire(dt);
                UpdateRequest(ref _fireRequest, done);
            }

            _dashTimer = Math.Max(0, _dashTimer - dt);
            if (_dashRequest != ActionRequestType.NotRequested)
            {
                bool done = DoDash();
                UpdateRequest(ref _dashRequest, done);
            }

            if (gameObject.transform.position.y < -1000)
            {
                health.ApplyDamageGeneral(500);
            }
        }

        private void FixedUpdate()
        {
            int groundMask = groundLayers.value;

            float dt = Time.fixedDeltaTime;
            _jumpTimer = Math.Max(0, _jumpTimer - dt);

            gameObject.transform.rotation = Quaternion.Euler(0, _lookYaw, 0);
            eyeObject.transform.localRotation = Quaternion.Euler(_lookPitch, 0, 0);

            Vector3 groundCheckerCenter = GetGroundCheckerCenter();
            _isGrounded = CheckGround(groundCheckerCenter, groundCheckerRadius, groundMask);

            _isGoodAngle = false;
            if (_isGrounded)
            {
                if (RaycastGround(groundCheckerCenter, groundCheckerRadius + normalCheckMaxDistance, out Vector3 normal,
                        groundMask))
                {
                    float slopeAngle = Vector3.Angle(normal, Vector3.up);
                    _isGoodAngle = slopeAngle <= maxSlopeAngle;
                }
            }

            if (_isGrounded && _isGoodAngle)
            {
                _hasDoubleJump = true;
            }

            Vector3 curVelocity = _rb.linearVelocity;
            Vector3 deltaVelocityXZ = _targetVelocity - curVelocity;
            deltaVelocityXZ.y = 0;
            float acceleration = _isGrounded ? moveAccelerationOnGround : moveAccelerationOnFly;
            Vector3 appliedDeltaVelocity = deltaVelocityXZ.normalized * acceleration;
            if (appliedDeltaVelocity.magnitude >= deltaVelocityXZ.magnitude)
            {
                curVelocity.x = _targetVelocity.x;
                curVelocity.z = _targetVelocity.z;
                _rb.linearVelocity = curVelocity;
            }
            else
            {
                _rb.AddForce(appliedDeltaVelocity, ForceMode.VelocityChange);
            }

            if (_jumpRequest != ActionRequestType.NotRequested)
            {
                bool done = DoJump();
                UpdateRequest(ref _jumpRequest, done);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 groundCheckerCenter = GetGroundCheckerCenter();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckerCenter, groundCheckerRadius);

            Vector3 end = groundCheckerCenter + Vector3.down * (groundCheckerRadius + normalCheckMaxDistance);
            Gizmos.DrawLine(groundCheckerCenter, end);

            Gizmos.DrawLine(eyeObject.transform.position,
                eyeObject.transform.position + eyeObject.transform.forward * 2f);
        }

        private bool DoJump()
        {
            if (_jumpTimer > 0)
            {
                return false;
            }

            if (!_isGrounded || !_isGoodAngle)
            {
                if (!_hasDoubleJump)
                {
                    return false;
                }

                _hasDoubleJump = false;
            }

            _jumpTimer = jumpReloadTime;

            float impulse = Mathf.Sqrt(jumpHeight * 2f * Physics.gravity.magnitude);
            _rb.AddForce(-impulse * Physics.gravity.normalized, ForceMode.VelocityChange);
            return true;
        }

        private bool DoDash()
        {
            if (_dashTimer > 0)
            {
                return false;
            }

            _dashTimer = dashReloadTime;

            Vector3 change = _dashTargetXZ * dashVelocityChange;
            _rb.AddForce(change, ForceMode.VelocityChange);
            return true;
        }

        private bool DoFire(float dt)
        {
            bool fired = _activeWeapon.TryFire();
            if (!fired)
            {
                return false;
            }

            Vector3 start = firePoint.transform.position;
            Vector3 lookDir = firePoint.transform.forward;

            Color color = Teams.ToColor(_relationshipsActor.Team);

            _projectileManager.Fire(gameObject, _activeWeapon.Meta, start, lookDir, color, dt, out Vector3 backImpulse);
            _rb.AddForceAtPosition(backImpulse, start, ForceMode.Impulse);

            NotifyFired();
            return true;
        }

        private void OnDied()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private bool CheckGround(Vector3 center, float radius, int mask)
        {
            var size = Physics.OverlapSphereNonAlloc(center, radius, _colliderBuffer, mask,
                QueryTriggerInteraction.Ignore);
            for (int i = 0; i < size; i++)
            {
                if (_colliderBuffer[i].gameObject == gameObject)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool RaycastGround(Vector3 origin, float distance, out Vector3 normal, int mask)
        {
            int numHits = Physics.RaycastNonAlloc(origin, Vector3.down, _raycastHitBuffer, distance, mask,
                QueryTriggerInteraction.Ignore);
            for (int i = 0; i < numHits; i++)
            {
                RaycastHit hit = _raycastHitBuffer[i];
                if (hit.collider.gameObject == gameObject)
                {
                    continue;
                }

                normal = hit.normal;
                return true;
            }

            normal = Vector3.zero;
            return false;
        }

        private Vector3 GetGroundCheckerCenter()
        {
            Vector3 pos = transform.position;
            pos.y -= groundCheckerOffset;
            return pos;
        }

        private void NotifyFired()
        {
            Fired?.Invoke();
        }

        private static void UpdateRequest(ref ActionRequestType type, bool done)
        {
            switch (type)
            {
                case ActionRequestType.TryNow:
                case ActionRequestType.DoWhenReadyAndFinish when done:
                    type = ActionRequestType.NotRequested;
                    break;
            }
        }
    }
}