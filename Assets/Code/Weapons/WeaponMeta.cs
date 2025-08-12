using System;
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

        public bool isDamageByTime;

        public float cooldownTime = 0.4f;
        public float bulletDamage = 12f;
        public int numBullets = 1;

        public float bulletImpulse = 20f;
        public float bulletBackImpulse = 2f;

        public float bulletLifeTime = 2.5f;
        public float bulletSpeed = 170f;
        public float spread = 0.1f;

        public float bulletReboundChance = 0.6f;
        public float multiplierByRebound = 0.4f;
        public float bulletGravityFactor = 1f;

        public int ammoInMagazine = 30;
        public int initialAmmo = 400;
    }
}