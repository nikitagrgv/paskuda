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
                _reloadTimer = 0f;
                _ammoInMagazine = _meta.ammoInMagazine;
                _totalAmmo = _meta.initialAmmo;
            }
        }

        public bool IsReadyToFire => _cooldownTimer <= 0 && _reloadTimer <= 0;
        public bool IsCoolingDown => _cooldownTimer > 0;
        public bool IsReloading => _reloadTimer > 0;

        public float RemainingCooldownTimeNormalized =>
            _meta.cooldownTime == 0 ? 0 : Mathf.Clamp01(_cooldownTimer / _meta.cooldownTime);

        public float RemainingReloadTimeNormalized =>
            _meta.reloadTime == 0 ? 0 : Mathf.Clamp01(_reloadTimer / _meta.reloadTime);

        public float RemainingTimeNormalized =>
            _reloadTimer > 0 ? RemainingReloadTimeNormalized : RemainingCooldownTimeNormalized;

        public int AmmoInMagazine => _ammoInMagazine;
        public int TotalAmmo => _totalAmmo;
        public int AmmoNotInMagazine => _totalAmmo - _ammoInMagazine;

        private WeaponMeta _meta;
        private float _cooldownTimer;
        private float _reloadTimer;
        private int _ammoInMagazine;
        private int _totalAmmo;

        public bool TryFire()
        {
            if (!IsReadyToFire)
            {
                return false;
            }

            if (_totalAmmo == 0 || _ammoInMagazine == 0)
            {
                return false;
            }

            _ammoInMagazine--;
            _totalAmmo--;
            if (_ammoInMagazine == 0)
            {
                _reloadTimer = _meta.reloadTime;
                return true;
            }

            _cooldownTimer = _meta.cooldownTime;
            return true;
        }

        public void Update(float dt)
        {
            _cooldownTimer = Mathf.Clamp(_cooldownTimer - dt, 0, _meta.cooldownTime);
            _reloadTimer = Mathf.Clamp(_reloadTimer - dt, 0, _meta.reloadTime);

            if (_ammoInMagazine == 0 && _totalAmmo != 0 && !IsReloading)
            {
                _ammoInMagazine = Math.Min(_meta.ammoInMagazine, _totalAmmo);
            }
        }
    }
}