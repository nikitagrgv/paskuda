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
            public Projectile Projectile;
            public Vector3 Velocity;
            public float TimeToLive;
            public float EffectMultiplier;
        }

        private class WeaponProjectiles
        {
            public readonly List<ProjectileInfo> Active = new();
            public readonly List<ProjectileInfo> Dying = new();
            public readonly List<Projectile> Pool = new();
        }

        private Dictionary<WeaponMeta, WeaponProjectiles> _projectilesByWeapon = new();

        public void Fire(GameObject sender, WeaponMeta weapon, Vector3 start, Vector3 dir, Color color, float dt,
            out Vector3 backImpulse)
        {
            WeaponProjectiles wp = _projectilesByWeapon.GetOrCreate(weapon);

            int numBullets = weapon.numBullets;

            float effectMultiplier = weapon.isDamageByTime ? dt * weapon.bulletDamage : 1f;

            float backImpulseByBullet = weapon.bulletBackImpulse * effectMultiplier / numBullets;
            backImpulse = new Vector3();
            for (int i = 0; i < numBullets; i++)
            {
                Projectile projectile = SpawnProjectile(weapon, wp);
                projectile.SetColor(color);

                Vector3 newDir = dir.WithSpread(weapon.spread);

                projectile.transform.position = start;
                projectile.transform.rotation = Quaternion.LookRotation(newDir);

                Vector3 velocity = newDir * weapon.bulletSpeed;

                ProjectileInfo info = new()
                {
                    Sender = sender,
                    Projectile = projectile,
                    Velocity = velocity,
                    TimeToLive = weapon.bulletLifeTime,
                    EffectMultiplier = effectMultiplier,
                };
                wp.Active.Add(info);

                backImpulse -= newDir * backImpulseByBullet;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (KeyValuePair<WeaponMeta, WeaponProjectiles> p in _projectilesByWeapon)
            {
                UpdateDying(p.Value, dt);
                UpdateActive(p.Key, p.Value, dt);
            }
        }

        private void UpdateDying(WeaponProjectiles wp, float dt)
        {
            List<ProjectileInfo> dying = wp.Dying;

            int count = dying.Count;
            for (int i = 0; i < count; i++)
            {
                ProjectileInfo info = dying[i];
                info.TimeToLive -= dt;
                if (info.TimeToLive <= 0)
                {
                    dying.RemoveAtSwapBack(i);
                    RemoveProjectile(info.Projectile, wp);
                    i--;
                    count--;
                    continue;
                }

                dying[i] = info;
            }
        }

        private void UpdateActive(WeaponMeta weapon, WeaponProjectiles wp, float dt)
        {
            List<ProjectileInfo> active = wp.Active;

            Vector3 gravity = Physics.gravity;
            int mask = interactableLayers.value;
            int count = active.Count;
            for (int i = 0; i < count; i++)
            {
                ProjectileInfo info = active[i];
                info.TimeToLive -= dt;
                if (info.TimeToLive <= 0)
                {
                    active.RemoveAtSwapBack(i);
                    MoveToDying(info, wp);
                    i--;
                    count--;
                    continue;
                }

                info.Velocity += weapon.bulletGravityFactor * dt * gravity;

                Vector3 oldPosition = info.Projectile.transform.position;
                float speed = info.Velocity.magnitude;
                Vector3 dir = info.Velocity / speed;
                if (Physics.Raycast(oldPosition, dir, out RaycastHit hit, speed * dt, mask,
                        QueryTriggerInteraction.Ignore))
                {
                    ApplyHit(hit, weapon, info);

                    float reboundChance = weapon.bulletReboundChance;
                    if (reboundChance > 0f && Utils.TryChance(reboundChance))
                    {
                        info.Velocity = Vector3.Reflect(info.Velocity, hit.normal);
                        info.Projectile.transform.position = hit.point + dir * 0.01f;
                        info.EffectMultiplier *= weapon.multiplierByRebound;
                        active[i] = info;
                        continue;
                    }

                    info.Projectile.transform.position = hit.point;
                    active.RemoveAtSwapBack(i);
                    MoveToDying(info, wp);
                    i--;
                    count--;
                    continue;
                }

                info.Projectile.transform.position = oldPosition + dt * info.Velocity;
                active[i] = info;
            }
        }

        private static void ApplyHit(RaycastHit hit, WeaponMeta weapon, ProjectileInfo info)
        {
            float numBullets = weapon.numBullets;
            float multiplier = info.EffectMultiplier / numBullets;

            float impulse = weapon.bulletImpulse * multiplier;
            hit.rigidbody?.AddForceAtPosition(info.Velocity.normalized * impulse, hit.point, ForceMode.Impulse);

            GameObject go = hit.collider?.gameObject;
            if (!go) return;

            Health health = go.GetComponentInParent<Health>();
            if (!health) return;

            float damage = weapon.bulletDamage * multiplier;
            health.ApplyDamageHit(damage, info.Sender);
        }

        private void MoveToDying(ProjectileInfo info, WeaponProjectiles wp)
        {
            info.TimeToLive = 1f;
            info.Projectile.MakeCrashed();
            wp.Dying.Add(info);
        }

        private Projectile SpawnProjectile(WeaponMeta weapon, WeaponProjectiles wp)
        {
            List<Projectile> pool = wp.Pool;

            if (pool.Count <= 0)
            {
                return Instantiate(weapon.projectilePrefab);
            }

            Projectile projectile = pool[^1];
            pool.RemoveAt(pool.Count - 1);
            projectile.MakeVisible();
            return projectile;
        }

        private void RemoveProjectile(Projectile projectile, WeaponProjectiles wp)
        {
            List<Projectile> pool = wp.Pool;

            projectile.MakeHidden();
            pool.Add(projectile);
        }
    }
}