using System;
using System.Collections.Generic;
using System.Linq;
using Code.Components;
using UnityEngine;

namespace Code.Weapons
{
    [Serializable]
    [CreateAssetMenu(menuName = "Game/Weapon")]
    public class WeaponMeta : ScriptableObject
    {
        public string weaponName = "No name";

        public Projectile projectilePrefab;

        public float fireReloadTime = 0.4f;
        public float bulletDamage = 12f;
        public int numBullets = 1;

        public float bulletImpulse = 20f;
        public float bulletBackImpulse = 2f;

        public float bulletLifeTime = 2.5f;
        public float bulletSpeed = 170f;
        public float spread = 0.1f;

        public float bulletReboundChance = 0.6f;
        public float bulletGravityFactor = 1f;

        public Projectile SpawnProjectile()
        {
            if (_projectiles.Count <= 0)
            {
                return Instantiate(projectilePrefab);
            }

            Projectile projectile = _projectiles.Last();
            _projectiles.RemoveAt(_projectiles.Count - 1);
            projectile.MakeVisible();
            return projectile;
        }

        public void RemoveProjectile(Projectile projectile)
        {
            projectile.MakeHidden();
            _projectiles.Add(projectile);
        }

        public void CrashProjectile(Projectile projectile)
        {
            projectile.MakeCrashed();
        }

        [NonSerialized]
        private List<Projectile> _projectiles = new();
    }
}