using System;
using UnityEngine;

namespace Code.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpulseAware : MonoBehaviour
    {
        public float impulseMultiplier = 2f;
        public string multiplierIgnoreTag = "Ground";
        public bool applyImpulseOnlyWhenNonKinematic = true;

        public AnimationCurve impulseToDamageCurve = new AnimationCurve(
            new Keyframe(12f, 4f),
            new Keyframe(50f, 50f)
        )
        {
            preWrapMode = WrapMode.Clamp,
            postWrapMode = WrapMode.Clamp
        };

        private Rigidbody _rb;
        private Health _health;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float multiplier = collision.gameObject.CompareTag(multiplierIgnoreTag) ? 1f : impulseMultiplier;
            bool ignoreImpulseAdding = Mathf.Approximately(multiplier, 1f) ||
                                       (applyImpulseOnlyWhenNonKinematic &&
                                        (!collision.rigidbody || collision.rigidbody.isKinematic));
            
            float fullImpulseMagnitude = collision.impulse.magnitude * multiplier;
            ApplyDamage(fullImpulseMagnitude);

            if (ignoreImpulseAdding)
                return;

            multiplier -= 1f;
            
            int contactsCount = collision.contactCount;
            for (int i = 0; i < contactsCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                Vector3 impulse = contact.impulse;
                Vector3 normal = contact.normal;
                Vector3 point = contact.point;

                float mul = multiplier;
                if (Vector3.Dot(normal, impulse) < 0f)
                    mul = -mul;

                impulse *= mul;
                _rb.AddForceAtPosition(impulse, point, ForceMode.Impulse);
            }
        }

        private void ApplyDamage(float impulseMagnitude)
        {
            if (!_health)
            {
                return;
            }

            float damage = impulseToDamageCurve.Evaluate(impulseMagnitude);
            if (damage <= 0.1)
            {
                return;
            }

            _health.ApplyDamageImpulsive(damage);
        }
    }
}