using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Components
{
    public class VisibilityChecker : MonoBehaviour
    {
        public ReadOnlyCollection<GameObject> VisibleObjects => _visibleObjects.AsReadOnly();

        private readonly HashSet<GameObject> _visibleObjectsSet = new(5);
        private readonly List<GameObject> _visibleObjects = new(5);

        private void Update()
        {
            _visibleObjects.Clear();
            _visibleObjectsSet.RemoveWhere(obj =>
            {
                bool deleted = !obj;
                if (!deleted)
                {
                    _visibleObjects.Add(obj);
                }

                return deleted;
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            _visibleObjectsSet.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            _visibleObjectsSet.Remove(other.gameObject);
        }
    }
}