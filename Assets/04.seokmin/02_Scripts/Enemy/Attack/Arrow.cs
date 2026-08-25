using RailGame.Enemy.Temp;
using UnityEngine;

namespace RailGame.Enemy.Attack
{
    [RequireComponent(typeof(Collider))]
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 5f;

        private float damage;
        private Vector3 direction;

        public void Launch(Vector3 travelDirection, float attackPower)
        {
            direction = travelDirection;
            damage = attackPower;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return;

            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}