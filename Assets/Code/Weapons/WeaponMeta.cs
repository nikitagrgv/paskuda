using System;
using System.Collections.Generic;
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

        public float bulletImpulse = 20f;
        public float bulletBackImpulse = 2f;

        public float bulletLifeTime = 2.5f;
        public float bulletSpeed = 170f;

        public float bulletReboundChance = 0.6f;

        public Projectile SpawnProjectile()
        {
            return Instantiate(projectilePrefab);
        }

        public void RemoveProjectile(Projectile projectile)
        {
            Destroy(projectile.gameObject);
        }

        [NonSerialized]
        private List<Projectile> _projectiles;
    }
}