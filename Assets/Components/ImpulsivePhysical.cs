using System;
using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpulsivePhysical : MonoBehaviour
    {
        public float impulseMultiplier = 2f;
        public bool ignoreKinematic = true;
        public string ignoreTag = "Ground";

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(ignoreTag))
            {
                return;
            }

            if (ignoreKinematic && (other.rigidbody == null || other.rigidbody.isKinematic))
            {
                return;
            }

            Vector3 initImpulse = other.impulse;
            initImpulse *= (impulseMultiplier - 1);
            // TODO: AddForceAtPosition!
            _rb.AddForce(initImpulse, ForceMode.Impulse);
        }
    }
}