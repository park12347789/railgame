using System;
using RailGame.Enemy.Temp;
using UnityEngine;

namespace Railgame.Player
{
    [DisallowMultipleComponent]
    public sealed class RailgamePlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            ResetHealth();
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsDead)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (IsDead)
                OnDied?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
