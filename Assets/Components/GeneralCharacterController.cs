using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class GeneralCharacterController : MonoBehaviour
    {
        public const float MinPitch = -87f;
        public const float MaxPitch = 87f;

        public enum ActionRequestType
        {
            NotRequested,
            TryNow,
            DoWhenReady,
        }

        public GameObject eyeObject;
        public GameObject firePoint;

        [Header("Movement")]
        public float moveAccelerationOnGround = 0.7f;

        public float moveAccelerationOnFly = 0.55f;
        public float jumpHeight = 2f;
        public float jumpReloadTime = 0.1f;
        public float maxSlopeAngle = 45f;

        public float dashVelocityChange = 20f;
        public float dashReloadTime = 2f;

        [Header("Ground Check")]
        public float groundCheckerRadius = 0.49f;

        public float groundCheckerOffset = 0.57f;
        public float normalCheckMaxDistance = 0.5f;

        [Header("Weapons")]
        public Projectile projectilePrefab;

        public float fireReloadTime = 1f;
        public float bulletLifeTime = 4f;
        public float bulletSpeed = 100f;
        public float bulletImpulse = 40f;
        public float bulletReboundChance = 0.9f;
        public float bulletBackImpulse = 10f;
        public float bulletDamage = 10f;

        public float LookPitch
        {
            get => _lookPitch;
            set => _lookPitch = ToValidPitch(value);
        }

        public float LookYaw
        {
            get => _lookYaw;
            set => _lookYaw = ToValidYaw(value);
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

        public ActionRequestType DashRequest
        {
            get => _dashRequest;
            set => _dashRequest = value;
        }

        public Vector3 TargetVelocity
        {
            get => _targetVelocity;
            set => _targetVelocity = value;
        }

        public bool IsDashReady => _dashTimer <= 0;
        public bool IsJumpReady => _jumpTimer <= 0 && _hasDoubleJump;
        public float RemainingReloadTimeNormalized => Mathf.Clamp(_fireTimer / fireReloadTime, 0f, 1f);

        private ProjectileManager _projectileManager;

        private Rigidbody _rb;
        private RelationshipsActor _relationshipsActor;
        private Health _health;

        private float _lookPitch;
        private float _lookYaw;

        private Vector3 _targetVelocity = Vector3.zero;
        private bool _isGrounded;
        private bool _isGoodAngle;

        private ActionRequestType _jumpRequest = ActionRequestType.NotRequested;
        private float _jumpTimer;
        private bool _hasDoubleJump;

        private ActionRequestType _fireRequest = ActionRequestType.NotRequested;
        private float _fireTimer;

        private ActionRequestType _dashRequest = ActionRequestType.NotRequested;
        private float _dashTimer;

        private readonly Collider[] _colliderBuffer = new Collider[10];
        private readonly RaycastHit[] _raycastHitBuffer = new RaycastHit[10];

        private void Start()
        {
            GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
            Assert.IsNotNull(gameController);
            _projectileManager = gameController?.GetComponent<ProjectileManager>();

            _rb = GetComponent<Rigidbody>();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _health = GetComponent<Health>();
            _health.Died += OnDied;

            _relationshipsActor = GetComponent<RelationshipsActor>();
            UpdateColorFromTeam();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            eyeObject.transform.rotation = Quaternion.Euler(_lookPitch, _lookYaw, 0);

            _fireTimer = Math.Max(0, _fireTimer - dt);
            if (_fireRequest != ActionRequestType.NotRequested)
            {
                if (_fireRequest == ActionRequestType.TryNow)
                {
                    _fireRequest = ActionRequestType.NotRequested;
                }

                if (_fireTimer <= 0)
                {
                    _fireRequest = ActionRequestType.NotRequested;
                    DoFire();
                    _fireTimer = fireReloadTime;
                }
            }

            _dashTimer = Math.Max(0, _dashTimer - dt);
            if (_dashRequest != ActionRequestType.NotRequested)
            {
                if (_dashRequest == ActionRequestType.TryNow)
                {
                    _dashRequest = ActionRequestType.NotRequested;
                }

                if (_dashTimer <= 0 && _targetVelocity.sqrMagnitude > 0.01)
                {
                    _dashRequest = ActionRequestType.NotRequested;
                    DoDash();
                    _dashTimer = dashReloadTime;
                }
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _jumpTimer = Math.Max(0, _jumpTimer - dt);

            gameObject.transform.rotation = Quaternion.Euler(0, _lookYaw, 0);
            eyeObject.transform.rotation = Quaternion.Euler(_lookPitch, _lookYaw, 0);

            Vector3 groundCheckerCenter = GetGroundCheckerCenter();
            _isGrounded = CheckGround(groundCheckerCenter, groundCheckerRadius);

            _isGoodAngle = false;
            if (_isGrounded)
            {
                if (RaycastGround(groundCheckerCenter, groundCheckerRadius + normalCheckMaxDistance,
                        out Vector3 normal))
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
                if (_jumpRequest == ActionRequestType.TryNow)
                {
                    _jumpRequest = ActionRequestType.NotRequested;
                }

                bool canDoJump = _isGrounded && _isGoodAngle && _jumpTimer <= 0;
                if (!canDoJump && _hasDoubleJump && _jumpTimer <= 0)
                {
                    canDoJump = true;
                    _hasDoubleJump = false;
                }

                if (canDoJump)
                {
                    _jumpRequest = ActionRequestType.NotRequested;

                    _jumpTimer = jumpReloadTime;
                    float impulse = Mathf.Sqrt(jumpHeight * 2f * Physics.gravity.magnitude);
                    _rb.AddForce(-impulse * Physics.gravity.normalized, ForceMode.VelocityChange);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 groundCheckerCenter = GetGroundCheckerCenter();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckerCenter, groundCheckerRadius);

            Vector3 end = groundCheckerCenter + Vector3.down * (groundCheckerRadius + normalCheckMaxDistance);
            Gizmos.DrawLine(groundCheckerCenter, end);
        }

        private void DoDash()
        {
            Vector3 change = _targetVelocity.normalized * dashVelocityChange;
            _rb.AddForce(change, ForceMode.VelocityChange);
        }

        private void DoFire()
        {
            Projectile projectile = SpawnProjectile();

            Vector3 start = firePoint.transform.position;
            Vector3 lookDir = firePoint.transform.forward;
            Vector3 resultVelocity = _rb.linearVelocity + lookDir * bulletSpeed;
            float resultSpeed = resultVelocity.magnitude;
            Vector3 dir = resultVelocity / resultSpeed;

            _projectileManager.AddProjectile(gameObject, projectile, start, dir, bulletLifeTime, bulletDamage,
                resultSpeed,
                bulletImpulse,
                bulletReboundChance);
            _rb.AddForceAtPosition(-lookDir * bulletBackImpulse, start, ForceMode.Impulse);
        }

        private Projectile SpawnProjectile()
        {
            Projectile projectile = Instantiate(projectilePrefab);
            Color color = Teams.ToColor(_relationshipsActor.Team);
            projectile.SetColor(color);
            return projectile;
        }


        private void OnDied()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void UpdateColorFromTeam()
        {
            Color color = Teams.ToColor(_relationshipsActor.Team);
            // color = Color.Lerp(color, Color.gray, 0.0f);
            if (_relationshipsActor != null)
            {
                UpdateColorRecursively(transform, color);
            }
        }

        private static void UpdateColorRecursively(Transform transform, Color color)
        {
            GameObject go = transform.gameObject;

            Renderer rnd = go.GetComponent<Renderer>();
            if (rnd)
            {
                rnd.material.color = color;
            }

            foreach (Transform child in transform)
            {
                UpdateColorRecursively(child, color);
            }
        }

        private bool CheckGround(Vector3 center, float radius)
        {
            var size = Physics.OverlapSphereNonAlloc(center, radius, _colliderBuffer);
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

        private bool RaycastGround(Vector3 origin, float distance, out Vector3 normal)
        {
            int numHits = Physics.RaycastNonAlloc(origin, Vector3.down, _raycastHitBuffer, distance);
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

        static float ToValidYaw(float angle)
        {
            angle %= 360f;
            if (angle < 0)
            {
                angle += 360f;
            }

            return angle;
        }

        static float ToValidPitch(float angle)
        {
            return Mathf.Clamp(angle, MinPitch, MaxPitch);
        }
    }
}