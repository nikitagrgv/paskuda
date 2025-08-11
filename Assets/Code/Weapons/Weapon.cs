using System;
using Code.Components;
using UnityEngine;

namespace Code.Weapons
{
    [Serializable]
    public class Weapon
    {
        public string name;

        public Projectile projectilePrefab;

        public float fireReloadTime = 0.4f;

        public float bulletLifeTime = 2.5f;
        public float bulletSpeed = 170f;
        public float bulletImpulse = 20f;
        public float bulletReboundChance = 0.6f;
        public float bulletBackImpulse = 2f;
        public float bulletDamage = 12f;
    }
}