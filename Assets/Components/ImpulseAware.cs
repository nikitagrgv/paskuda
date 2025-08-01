using System;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpulseAware : MonoBehaviour
    {
        public float impulseMultiplier = 2f;
        public string multiplierIgnoreTag = "Ground";
        public bool ignoreKinematic = true;

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

        private void OnCollisionEnter(Collision other)
        {
            bool multiplyIgnored = other.gameObject.CompareTag(multiplierIgnoreTag);
            ApplyDamage(other.impulse.magnitude * (multiplyIgnored ? 1 : impulseMultiplier));

            if (multiplyIgnored)
            {
                return;
            }

            if (Mathf.Approximately(impulseMultiplier, 1f))
            {
                return;
            }

            if (ignoreKinematic && (!other.rigidbody || other.rigidbody.isKinematic))
            {
                return;
            }

            Vector3 initImpulse = other.impulse;
            initImpulse *= (impulseMultiplier - 1);
            // TODO: AddForceAtPosition!
            _rb.AddForce(initImpulse, ForceMode.Impulse);
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

            _health.ApplyDamage(damage);
        }
    }
}