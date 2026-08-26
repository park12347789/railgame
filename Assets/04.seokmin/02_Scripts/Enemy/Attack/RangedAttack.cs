using UnityEngine;

namespace RailGame.Enemy.Attack
{
    public class RangedAttack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private Arrow arrowPrefab;
        [SerializeField] private Transform firePoint;

        private float attackPower;
        private float attackCooldown;
        private float lastAttackTime = -999f;

        public bool IsBusy => false;
        public bool ManagesOwnEngagement => false;

        public void Initialize(float power, float cooldown)
        {
            attackPower = power;
            attackCooldown = cooldown;
        }

        public bool TryAttack(Transform target)
        {
            if (Time.time - lastAttackTime < attackCooldown) return false;
            if (arrowPrefab == null || firePoint == null) return false;

            lastAttackTime = Time.time;

            Vector3 direction = (target.position - firePoint.position).normalized;
            Arrow arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.LookRotation(direction));
            arrow.Launch(direction, attackPower, transform);

            return true;
        }
    }
}