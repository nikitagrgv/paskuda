using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    public class VisibilityChecker : MonoBehaviour
    {
        public HashSet<GameObject> VisibleObjects { get; } = new();

        private void Update()
        {
            VisibleObjects.RemoveWhere(obj => !obj);
        }

        private void OnTriggerEnter(Collider other)
        {
            VisibleObjects.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            VisibleObjects.Remove(other.gameObject);
        }
    }
}