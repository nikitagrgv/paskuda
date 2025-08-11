using System;
using System.Collections.Generic;
using Code.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Components
{
    public class Arsenal : MonoBehaviour
    {
        [Serializable]
        public class ArsenalWeapon
        {
            public WeaponMeta weapon;
            public float chanceFactor = 1f;
        }

        public List<ArsenalWeapon> weapons = new();

        private bool _calculated;
        private float _totalChance;

        public WeaponMeta GetRandomWeapon()
        {
            if (!_calculated)
            {
                Recalculate();
            }

            float rand = Random.Range(0f, _totalChance);

            float accumulated = 0f;
            foreach (ArsenalWeapon arsenalWeapon in weapons)
            {
                accumulated += arsenalWeapon.chanceFactor;
                if (rand < accumulated)
                {
                    return arsenalWeapon.weapon;
                }
            }

            return weapons[^1].weapon;
        }

        private void OnDestroy()
        {
            foreach (ArsenalWeapon weapon in weapons)
            {
                weapon.weapon.ClearProjectiles();
            }
        }

        private void Recalculate()
        {
            _calculated = true;

            _totalChance = 0f;
            foreach (ArsenalWeapon arsenalWeapon in weapons)
            {
                _totalChance += arsenalWeapon.chanceFactor;
            }
        }
    }
}