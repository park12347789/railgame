using System;
using RailGame.Enemy.Data;
using UnityEngine;

namespace RailGame.Enemy.Runtime
{
    /// <summary>
    /// 실제 전투 중 변하는 적 개별 스탯.
    /// EnemyDataSO는 여러 적이 공유하지만, 이 클래스는 적 인스턴스마다 하나씩 생성해서 소유한다.
    /// 웨이브 스케일링, 버프/디버프는 전부 이 클래스의 값을 통해서만 적용한다.
    /// </summary>
    [Serializable]
    public class EnemyRuntimeStats
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float AttackPower { get; private set; }
        public float AttackRange { get; private set; }
        public float AttackCooldown { get; private set; }
        public float MoveSpeed { get; private set; }

        public bool IsDead => CurrentHealth <= 0f;

        /// <summary>
        /// 체력이 변할 때 알림 (UI 체력바 등에서 구독)
        /// </summary>
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action OnDied;

        public EnemyRuntimeStats(EnemyDataSO data, float waveMultiplier = 1f)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "EnemyDataSO가 null입니다.");
            }

            // 웨이브 스케일링 등은 여기서 배수만 곱해서 초기값을 만든다.
            // SO 원본 값은 절대 수정하지 않는다.
            MaxHealth = data.maxHealth * waveMultiplier;
            CurrentHealth = MaxHealth;
            AttackPower = data.attackPower * waveMultiplier;
            AttackRange = data.attackRange;
            AttackCooldown = data.attackCooldown;
            MoveSpeed = data.moveSpeed;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        // 이후 버프/디버프 시스템에서 아래 형태로 확장 예정
        // (예: 슬로우 적용 시 MoveSpeed에 배수 적용 등)
        public void ModifyMoveSpeed(float multiplier)
        {
            MoveSpeed = Mathf.Max(0f, MoveSpeed * multiplier);
        }
    }
}