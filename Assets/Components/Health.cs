using System;
using UnityEngine;

namespace Components
{
    public class Health : MonoBehaviour
    {
        public event Action HealthChanged;
        public event Action Died;

        public float MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = Mathf.Max(1f, value);
        }

        public bool god = false;

        public float CurrentHealth => _currentHealth;
        public float CurrentHealthPercentage => _currentHealth / _maxHealth;
        public bool IsDead => _currentHealth <= 0 && !god;

        private float _maxHealth = 100f;
        private float _currentHealth;

        public void Start()
        {
            _currentHealth = _maxHealth;
        }

        public void AddHealth(float amount)
        {
            if (IsDead)
            {
                return;
            }

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
            if (oldHealth != _currentHealth)
            {
                NotifyHealthChanged();
            }

            if (IsDead)
            {
                NotifyDied();
            }
        }

        public void ApplyDamage(float amount)
        {
            AddHealth(-amount);
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke();
        }

        protected virtual void NotifyDied()
        {
            Died?.Invoke();
        }
    }
}