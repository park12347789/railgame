using RailGame.Enemy.Temp;
using UnityEngine;

namespace RailGame.Enemy.Attack
{
    public class MeleeAttack : MonoBehaviour, IEnemyAttack
    {
        private float attackPower;
        private float attackCooldown;
        private float lastAttackTime = -999f;

        public void Initialize(float power, float cooldown)
        {
            attackPower = power;
            attackCooldown = cooldown;
        }

        public bool TryAttack(Transform target)
        {
            if (Time.time - lastAttackTime < attackCooldown) return false;

            lastAttackTime = Time.time;
            target.GetComponent<IDamageable>()?.TakeDamage(attackPower);
            return true;
        }
    }
}