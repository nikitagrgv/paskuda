using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Components
{
    public class VisibilityChecker : MonoBehaviour
    {
        public static float NearWidth = 1f;
        public static float NearHeight = 1f;
        public static float FarWidth = 2f;
        public static float FarHeight = 2f;
        public static float Depth = 3f;

        public static Mesh FrustumMesh
        {
            get
            {
                if (!_frustumMesh)
                {
                    _frustumMesh = CreateFrustumMesh();
                }

                return _frustumMesh;
            }
        }

        public ReadOnlyCollection<GameObject> VisibleObjects => _visibleObjects.AsReadOnly();

        private readonly HashSet<GameObject> _visibleObjectsSet = new(5);
        private readonly List<GameObject> _visibleObjects = new(5);

        private static Vector3[] _frustumVertices;
        private static Mesh _frustumMesh;

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

        private void OnDrawGizmosSelected()
        {
            var mesh = FrustumMesh;
            if (_frustumVertices == null || _frustumVertices.Length < 8)
                return;

            Gizmos.color = Color.yellow;
            Transform t = transform;

            // Near plane
            DrawQuad(t, 0, 1, 2, 3);
            // Far plane
            DrawQuad(t, 4, 5, 6, 7);
            // Connect edges
            DrawLine(t, 0, 4);
            DrawLine(t, 1, 5);
            DrawLine(t, 2, 6);
            DrawLine(t, 3, 7);
        }

        private void DrawLine(Transform t, int i0, int i1)
        {
            Gizmos.DrawLine(t.TransformPoint(_frustumVertices[i0]), t.TransformPoint(_frustumVertices[i1]));
        }

        private static void DrawQuad(Transform t, int i0, int i1, int i2, int i3)
        {
            Gizmos.DrawLine(t.TransformPoint(_frustumVertices[i0]), t.TransformPoint(_frustumVertices[i1]));
            Gizmos.DrawLine(t.TransformPoint(_frustumVertices[i1]), t.TransformPoint(_frustumVertices[i2]));
            Gizmos.DrawLine(t.TransformPoint(_frustumVertices[i2]), t.TransformPoint(_frustumVertices[i3]));
            Gizmos.DrawLine(t.TransformPoint(_frustumVertices[i3]), t.TransformPoint(_frustumVertices[i0]));
        }

        private static Mesh CreateFrustumMesh()
        {
            Mesh mesh = new()
            {
                name = "FrustumMesh"
            };

            _frustumVertices = new Vector3[8];

            // Near plane (local z = 0)
            _frustumVertices[0] = new Vector3(-NearWidth / 2, -NearHeight / 2, 0); // Bottom Left
            _frustumVertices[1] = new Vector3(NearWidth / 2, -NearHeight / 2, 0); // Bottom Right
            _frustumVertices[2] = new Vector3(NearWidth / 2, NearHeight / 2, 0); // Top Right
            _frustumVertices[3] = new Vector3(-NearWidth / 2, NearHeight / 2, 0); // Top Left

            // Far plane (local z = depth)
            _frustumVertices[4] = new Vector3(-FarWidth / 2, -FarHeight / 2, Depth);
            _frustumVertices[5] = new Vector3(FarWidth / 2, -FarHeight / 2, Depth);
            _frustumVertices[6] = new Vector3(FarWidth / 2, FarHeight / 2, Depth);
            _frustumVertices[7] = new Vector3(-FarWidth / 2, FarHeight / 2, Depth);

            mesh.vertices = _frustumVertices;

            int[] triangles =
            {
                // Near
                0, 2, 1, 0, 3, 2,
                // Far
                4, 5, 6, 4, 6, 7,
                // Left
                0, 4, 7, 0, 7, 3,
                // Right
                1, 2, 6, 1, 6, 5,
                // Top
                2, 3, 7, 2, 7, 6,
                // Bottom
                0, 1, 5, 0, 5, 4
            };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}