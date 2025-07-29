using System;
using UnityEngine;

public class GeneralCharacterController : MonoBehaviour
{
    public enum JumpRequestType
    {
        TryJumpNow,
        JumpWhenReady,
        NotRequested
    }

    public GameObject eyeObject;

    [Header("Movement")]
    public float moveAccelerationOnGround = 15f;

    public float moveAccelerationOnFly = 3f;
    public float jumpHeight = 2f;
    public float jumpReloadTime = 0.2f;
    public float maxSlopeAngle = 45f;

    [Header("Ground Check")]
    public float groundCheckerRadius = 0.49f;

    public float groundCheckerOffset = 0.57f;
    public float normalCheckMaxDistance = 0.5f;

    public float LookPitch
    {
        get => _lookPitch;
        set => _lookPitch = value;
    }

    public float LookYaw
    {
        get => _lookYaw;
        set => _lookYaw = value;
    }

    public JumpRequestType JumpRequest
    {
        get => _jumpRequested;
        set => _jumpRequested = value;
    }

    public Vector3 TargetVelocityXZ
    {
        get => _targetVelocityXZ;
        set => _targetVelocityXZ = value;
    }

    private Rigidbody _rb;

    private float _lookPitch;
    private float _lookYaw;

    private Vector3 _targetVelocityXZ = Vector3.zero;
    private bool _isGrounded;
    private bool _isGoodAngle;
    private JumpRequestType _jumpRequested;
    private float _jumpTimer;

    private readonly Collider[] _colliderBuffer = new Collider[15];
    private readonly RaycastHit[] _raycastHitBuffer = new RaycastHit[15];

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        eyeObject.transform.rotation = Quaternion.Euler(_lookPitch, _lookYaw, 0);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        _jumpTimer = Math.Max(0, _jumpTimer - dt);

        Vector3 groundCheckerCenter = GetGroundCheckerCenter();
        _isGrounded = CheckGround(groundCheckerCenter, groundCheckerRadius);

        _isGoodAngle = false;
        if (_isGrounded)
        {
            if (RaycastGround(groundCheckerCenter, groundCheckerRadius + normalCheckMaxDistance, out Vector3 normal))
            {
                float slopeAngle = Vector3.Angle(normal, Vector3.up);
                _isGoodAngle = slopeAngle <= maxSlopeAngle;
            }
        }

        Vector3 curVelocity = _rb.linearVelocity;
        Vector3 deltaVelocityXZ = _targetVelocityXZ - curVelocity;
        deltaVelocityXZ.y = 0;
        if (deltaVelocityXZ.magnitude <= 0.1f)
        {
            curVelocity.x = _targetVelocityXZ.x;
            curVelocity.z = _targetVelocityXZ.z;
            _rb.linearVelocity = curVelocity;
        }
        else
        {
            float acceleration = _isGrounded ? moveAccelerationOnGround : moveAccelerationOnFly;
            _rb.AddForce(deltaVelocityXZ * acceleration, ForceMode.Acceleration);
        }


        if (_jumpRequested != JumpRequestType.NotRequested)
        {
            if (_jumpRequested == JumpRequestType.TryJumpNow)
            {
                _jumpRequested = JumpRequestType.NotRequested;
            }

            if (_isGrounded && _isGoodAngle && _jumpTimer == 0)
            {
                _jumpRequested = JumpRequestType.NotRequested;

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
        const float maxPitch = 87f;
        return Mathf.Clamp(angle, -maxPitch, maxPitch);
    }
}