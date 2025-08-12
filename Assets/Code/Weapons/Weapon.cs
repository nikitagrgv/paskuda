using System;
using UnityEngine;

namespace Code.Weapons
{
    public class Weapon
    {
        public WeaponMeta Meta
        {
            get => _meta;
            set
            {
                _meta = value;
                _cooldownTimer = 0f;
                _ammoInMagazine = _meta.ammoInMagazine;
                _totalAmmo = _meta.initialAmmo;
            }
        }

        public float CooldownTimer
        {
            get => _cooldownTimer;
            set => _cooldownTimer = Mathf.Clamp(value, 0, _meta.cooldownTime);
        }

        public bool IsReadyToFire => _cooldownTimer <= 0;

        public float RemainingCooldownTimeNormalized =>
            _meta.cooldownTime == 0 ? 0 : Mathf.Clamp01(_cooldownTimer / _meta.cooldownTime);

        private WeaponMeta _meta;
        private float _cooldownTimer;
        private int _ammoInMagazine;
        private int _totalAmmo;

        public bool TryFire()
        {
            if (!IsReadyToFire)
            {
                return false;
            }

            if (_ammoInMagazine == 0)
            {
                return false;
            }

            _ammoInMagazine--;
            if (_ammoInMagazine == 0)
            {
                int delta = Math.Min(_meta.ammoInMagazine, _totalAmmo);
                _ammoInMagazine = delta;
                _totalAmmo -= delta;
            }

            _cooldownTimer = _meta.cooldownTime;
            return true;
        }

        public void UpdateTimer(float dt)
        {
            CooldownTimer -= dt;
        }
    }
}