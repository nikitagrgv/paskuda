using System;
using System.Collections.Generic;
using Code.Weapons;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using Random = UnityEngine.Random;

namespace Code.Components
{
    public class ProjectileManager : MonoBehaviour
    {
        public LayerMask interactableLayers = Physics.AllLayers;

        private struct ProjectileInfo
        {
            public GameObject Sender;
            public WeaponMeta Weapon;
            public Projectile Projectile;
            public Vector3 Velocity;
            public float TimeToLive;
        }

        private readonly List<ProjectileInfo> _active = new();
        private readonly List<ProjectileInfo> _dying = new();

        public void AddProjectile(GameObject sender, WeaponMeta weapon, Vector3 start, Vector3 dir, Color color)
        {
            Projectile projectile = weapon.SpawnProjectile();
            projectile.SetColor(color);

            projectile.transform.position = start;
            projectile.transform.rotation = Quaternion.LookRotation(dir);

            Vector3 velocity = dir * weapon.bulletSpeed;

            ProjectileInfo info = new()
            {
                Sender = sender,
                Weapon = weapon,
                Projectile = projectile,
                Velocity = velocity,
                TimeToLive = weapon.bulletLifeTime,
            };
            _active.Add(info);
        }

        private void Update()
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
                    info.Weapon.RemoveProjectile(info.Projectile);
                    i--;
                    count--;
                    continue;
                }

                _dying[i] = info;
            }
        }

        private void UpdateActive(float dt)
        {
            Vector3 gravity = Physics.gravity;

            int mask = interactableLayers.value;

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

                info.Velocity += info.Weapon.bulletGravityFactor * dt * gravity;

                Vector3 oldPosition = info.Projectile.transform.position;
                float speed = info.Velocity.magnitude;
                Vector3 dir = info.Velocity / speed;
                if (Physics.Raycast(oldPosition, dir, out RaycastHit hit, speed * dt, mask,
                        QueryTriggerInteraction.Ignore))
                {
                    ApplyHit(hit, info);

                    float reboundChance = info.Weapon.bulletReboundChance;
                    if (reboundChance > 0f && Utils.TryChance(reboundChance))
                    {
                        info.Velocity = Vector3.Reflect(info.Velocity, hit.normal);
                        info.Projectile.transform.position = hit.point + dir * 0.01f;
                        _active[i] = info;
                        continue;
                    }

                    info.Projectile.transform.position = hit.point;
                    _active.RemoveAtSwapBack(i);
                    MoveToDying(info);
                    i--;
                    count--;
                    continue;
                }

                info.Projectile.transform.position = oldPosition + dt * info.Velocity;
                _active[i] = info;
            }
        }

        private static void ApplyHit(RaycastHit hit, ProjectileInfo info)
        {
            float impulse = info.Weapon.bulletImpulse;
            hit.rigidbody?.AddForceAtPosition(info.Velocity.normalized * impulse, hit.point, ForceMode.Impulse);

            GameObject go = hit.collider?.gameObject;
            if (!go) return;

            Health health = go.GetComponentInParent<Health>();
            if (!health) return;

            float damage = info.Weapon.bulletDamage;
            health.ApplyDamageHit(damage, info.Sender);
        }

        private void MoveToDying(ProjectileInfo info)
        {
            info.TimeToLive = 1f;
            info.Weapon.MakeCrashed(info.Projectile);
            _dying.Add(info);
        }
    }
}