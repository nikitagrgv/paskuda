using System;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpulseAware : MonoBehaviour
    {
        public float impulseMultiplier = 2f;
        public bool ignoreKinematic = true;
        public string ignoreTag = "Ground";

        public float damageImpulseThreshold = 12f;
        public float damageImpulseMax = 50f;
        public float minImpulseDamage = 4f;
        public float maxImpulseDamage = 50f;

        private Rigidbody _rb;
        private Health _health;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
        }

        private void OnCollisionEnter(Collision other)
        {
            ApplyDamage(other.impulse.magnitude * impulseMultiplier);

            if (Mathf.Approximately(impulseMultiplier, 1f))
            {
                return;
            }

            if (other.gameObject.CompareTag(ignoreTag))
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

            if (impulseMagnitude < damageImpulseThreshold)
            {
                return;
            }

            impulseMagnitude = Mathf.Min(impulseMagnitude, damageImpulseMax);

            float d = damageImpulseMax - damageImpulseThreshold;
            float k = maxImpulseDamage - minImpulseDamage;
            float b = damageImpulseMax * minImpulseDamage - damageImpulseThreshold * maxImpulseDamage;
            float damage = (impulseMagnitude * k + b) / d;

            _health.ApplyDamage(damage);
        }
    }
}