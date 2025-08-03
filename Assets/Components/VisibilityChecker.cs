using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Components
{
    public class VisibilityChecker : MonoBehaviour
    {
        public static float FieldOfView = 90f;
        public static float Aspect = 0.75f;
        public static float Near = 0.2f;
        public static float Far = 40f;

        public static Mesh FrustumMesh
        {
            get
            {
                CreateFrustumMeshLazy();
                return _frustumMesh;
            }
        }

        public ReadOnlyCollection<GameObject> VisibleObjects => _visibleObjects.AsReadOnly();

        private readonly HashSet<GameObject> _visibleObjectsSet = new(5);
        private readonly List<GameObject> _visibleObjects = new(5);

        private static Vector3[] _frustumVertices;
        private static Mesh _frustumMesh;

        private void Start()
        {
            GetComponent<MeshCollider>().sharedMesh = FrustumMesh;
        }

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
            Debug.Log(_visibleObjects.Count);
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
            CreateFrustumMeshLazy();
            if (_frustumVertices == null || _frustumVertices.Length < 8)
                return;

            Gizmos.color = Color.red;
            foreach (GameObject obj in _visibleObjects)
            {
                Gizmos.DrawSphere(obj.transform.position, 2f);
            }

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

        private static void CreateFrustumMeshLazy()
        {
            if (_frustumMesh)
            {
                return;
            }

            Mesh mesh = new()
            {
                name = "FrustumMesh"
            };

            float tanFov = Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);

            float nearWidth = 2f * Near * tanFov;
            float nearHeight = nearWidth / Aspect;

            float farWidth = 2f * Far * tanFov;
            float farHeight = farWidth / Aspect;

            _frustumVertices = new Vector3[8];

            // Near plane (z = Near)
            _frustumVertices[0] = new Vector3(-nearWidth / 2, -nearHeight / 2, Near); // Bottom Left
            _frustumVertices[1] = new Vector3(nearWidth / 2, -nearHeight / 2, Near); // Bottom Right
            _frustumVertices[2] = new Vector3(nearWidth / 2, nearHeight / 2, Near); // Top Right
            _frustumVertices[3] = new Vector3(-nearWidth / 2, nearHeight / 2, Near); // Top Left

            // Far plane (z = Far)
            _frustumVertices[4] = new Vector3(-farWidth / 2, -farHeight / 2, Far);
            _frustumVertices[5] = new Vector3(farWidth / 2, -farHeight / 2, Far);
            _frustumVertices[6] = new Vector3(farWidth / 2, farHeight / 2, Far);
            _frustumVertices[7] = new Vector3(-farWidth / 2, farHeight / 2, Far);

            mesh.vertices = _frustumVertices;

            int[] triangles =
            {
                0, 2, 1, 0, 3, 2, // Near
                4, 5, 6, 4, 6, 7, // Far
                0, 4, 7, 0, 7, 3, // Left
                1, 2, 6, 1, 6, 5, // Right
                2, 3, 7, 2, 7, 6, // Top
                0, 1, 5, 0, 5, 4 // Bottom
            };

            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _frustumMesh = mesh;
        }
    }
}