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
                _reloadTimer = 0f;
            }
        }

        public float ReloadTimer
        {
            get => _reloadTimer;
            set => _reloadTimer = Mathf.Clamp(value, 0, _meta.fireReloadTime);
        }

        public bool IsReadyToFire => _reloadTimer <= 0;

        public float RemainingReloadTimeNormalized =>
            _meta.fireReloadTime == 0 ? 0 : Mathf.Clamp01(_reloadTimer / _meta.fireReloadTime);

        private WeaponMeta _meta;
        private float _reloadTimer;

        public void Reload()
        {
            _reloadTimer = _meta.fireReloadTime;
        }

        public void UpdateTimer(float dt)
        {
            ReloadTimer -= dt;
        }
    }
}