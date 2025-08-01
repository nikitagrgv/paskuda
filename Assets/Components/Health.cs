using System;
using UnityEngine;

namespace Components
{
    public class Health : MonoBehaviour
    {
        public struct HealthChangeInfo
        {
            public float Delta;
            public GameObject Initiator;
            public bool IsHit;
            public bool IsImpulse;
        }

        public event Action<HealthChangeInfo> HealthChanged;
        public event Action Died;

        public static event Action<Health, HealthChangeInfo> AnyHealthChanged;
        public static event Action<Health> AnyDied;

        public float MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = Mathf.Max(1f, value);
        }

        public bool god = false;

        public ParticleSystem dieEffect;

        public float CurrentHealth => _currentHealth;
        public float CurrentHealthPercentage => _currentHealth / _maxHealth;
        public bool IsDead => _currentHealth <= 0 && !god;

        private float _maxHealth = 100f;
        private float _currentHealth;

        public void Start()
        {
            _currentHealth = _maxHealth;
        }

        public void RaiseHealthGeneral(float amount)
        {
            if (amount <= 0)
            {
                return;
            }

            HealthChangeInfo info = new() { Delta = amount };
            ChangeHealth(info);
        }

        public void ApplyDamageGeneral(float amount)
        {
            if (amount <= 0)
            {
                return;
            }

            HealthChangeInfo info = new() { Delta = -amount };
            ChangeHealth(info);
        }

        public void ApplyDamageImpulsive(float amount)
        {
            if (amount <= 0)
            {
                return;
            }

            HealthChangeInfo info = new() { Delta = -amount, IsImpulse = true };
            ChangeHealth(info);
        }

        public void ApplyDamageHit(float amount, GameObject sender)
        {
            if (amount <= 0)
            {
                return;
            }

            HealthChangeInfo info = new() { Delta = -amount, IsHit = true, Initiator = sender };
            ChangeHealth(info);
        }

        private void ChangeHealth(HealthChangeInfo info)
        {
            if (IsDead)
            {
                return;
            }

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth + info.Delta, 0, _maxHealth);
            info.Delta = _currentHealth - oldHealth;
            if (info.Delta != 0f)
            {
                NotifyHealthChanged(info);
            }

            if (IsDead)
            {
                if (dieEffect)
                {
                    ParticleSystem eff = Instantiate(dieEffect, transform.position, Quaternion.identity);
                    ParticleSystem.MainModule mm = eff.main;
                    mm.stopAction = ParticleSystemStopAction.Destroy;
                }

                NotifyDied();
            }
        }

        private void NotifyHealthChanged(HealthChangeInfo info)
        {
            HealthChanged?.Invoke(info);
            AnyHealthChanged?.Invoke(this, info);
        }

        protected virtual void NotifyDied()
        {
            Died?.Invoke();
            AnyDied?.Invoke(this);
        }
    }
}