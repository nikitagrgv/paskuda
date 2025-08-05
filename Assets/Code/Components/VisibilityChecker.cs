using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Code.Components
{
    public class VisibilityChecker : MonoBehaviour
    {
        const float FieldOfView = 90f;
        const float Aspect = 1f;
        const float Near = 0.2f;
        const float Far = 35f;

        private static Mesh FrustumMesh
        {
            get
            {
                CreateFrustumMeshLazy();
                return _frustumMesh;
            }
        }

        public event Action<GameObject, Info> BecomeVisible;
        public event Action<GameObject, Info> BecomeInvisible;

        public GameObject ignore;

        private bool _needInvalidationThisFrame = true;

        public struct Info
        {
            public RelationshipsActor RelationshipsActor;
            public Health Health;
        }

        public Dictionary<GameObject, Info> VisibleObjectsMap
        {
            get
            {
                if (_needInvalidationThisFrame)
                {
                    Invalidate();
                }

                return _visibleObjectsMap;
            }
        }

        private readonly Dictionary<GameObject, Info> _visibleObjectsMap = new(5);

        private static Vector3[] _frustumVertices;
        private static Mesh _frustumMesh;

        private void Start()
        {
            GetComponent<MeshCollider>().sharedMesh = FrustumMesh;
        }

        private void Invalidate()
        {
            List<GameObject> invalid = _visibleObjectsMap
                .Where(v => !v.Key)
                .Select(v => v.Key)
                .ToList();
            invalid.ForEach(v => { _visibleObjectsMap.Remove(v); });

            _needInvalidationThisFrame = false;
        }

        private void LateUpdate()
        {
            _needInvalidationThisFrame = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject go = other.gameObject;
            if (go == ignore)
            {
                return;
            }

            if (VisibleObjectsMap.ContainsKey(go))
            {
                return;
            }

            RelationshipsActor ra = go.GetComponent<RelationshipsActor>();
            Health health = go.GetComponent<Health>();
            if (!ra || !health)
            {
                return;
            }

            Info info = new() { RelationshipsActor = ra, Health = health };

            VisibleObjectsMap[go] = info;

            BecomeVisible?.Invoke(go, info);
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject go = other.gameObject;
            if (go == ignore)
            {
                return;
            }

            Info info = VisibleObjectsMap.GetValueAndRemove(go);
            BecomeInvisible?.Invoke(go, info);
        }

        private void OnDrawGizmosSelected()
        {
            CreateFrustumMeshLazy();
            if (_frustumVertices == null || _frustumVertices.Length < 8)
                return;

            Gizmos.color = Color.red;
            foreach (KeyValuePair<GameObject, Info> v in VisibleObjectsMap)
            {
                Gizmos.DrawSphere(v.Key.transform.position, 2f);
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