using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Components
{
    public class ProjectileManager : MonoBehaviour
    {
        public Projectile projectilePrefab;

        struct ProjectileInfo
        {
            public GameObject Sender;
            public Projectile Projectile;
            public float TimeToLive;
            public Vector3 Direction;
            public float Speed;
            public float Impulse;
        }

        private readonly List<ProjectileInfo> _active = new();
        private readonly List<ProjectileInfo> _dying = new();

        public void SpawnProjectile(GameObject sender, Vector3 start, Vector3 dir, float lifetime, float speed,
            float impulse)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            ProjectileInfo info = new ProjectileInfo
            {
                Sender = sender,
                Projectile = Instantiate(projectilePrefab, start, rot),
                TimeToLive = lifetime,
                Direction = dir,
                Speed = speed,
                Impulse = impulse
            };
            _active.Add(info);
        }

        public void Update()
        {
            float dt = Time.deltaTime;
            UpdateDying(dt);
            UpdateActive(dt);
        }

        private void UpdateDying(float dt)
        {
            int count = _dying.Count;
            for (int i = 0; i < count; i++)
            {
                ProjectileInfo info = _dying[i];
                info.TimeToLive -= dt;
                if (info.TimeToLive <= 0)
                {
                    _dying.RemoveAtSwapBack(i);
                    Destroy(info.Projectile);
                    i--;
                    count--;
                    continue;
                }

                _dying[i] = info;
            }
        }

        private void UpdateActive(float dt)
        {
            int count = _active.Count;
            for (int i = 0; i < count; i++)
            {
                ProjectileInfo info = _active[i];
                info.TimeToLive -= dt;
                if (info.TimeToLive <= 0)
                {
                    _active.RemoveAtSwapBack(i);
                    MoveToDying(info);
                    i--;
                    count--;
                    continue;
                }

                Vector3 oldPosition = info.Projectile.transform.position;
                if (Physics.Raycast(oldPosition, info.Direction, out RaycastHit hit, info.Speed * dt))
                {
                    hit.rigidbody?.AddForceAtPosition(info.Direction * info.Impulse, hit.point, ForceMode.Impulse);

                    info.Projectile.transform.position = hit.point;
                    _active.RemoveAtSwapBack(i);
                    MoveToDying(info);
                    i--;
                    count--;
                    continue;
                }

                info.Projectile.transform.position = oldPosition + info.Speed * dt * info.Direction;
                _active[i] = info;
            }
        }

        private void MoveToDying(ProjectileInfo info)
        {
            info.TimeToLive = 2f;
            _dying.Add(info);
        }
    }
}