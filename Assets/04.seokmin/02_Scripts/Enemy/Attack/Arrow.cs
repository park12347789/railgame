using RailGame.Enemy.Temp;
using UnityEngine;

namespace RailGame.Enemy.Attack
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float gravity = 9.8f;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float launchAngle = 12f;

        private float damage;
        private Vector3 velocity;
        private Transform owner;
        private bool isStuck;

        public void Launch(Vector3 travelDirection, float attackPower, Transform ownerTransform)
        {
            Vector3 flatDir = travelDirection;
            flatDir.y = 0f;
            flatDir.Normalize();

            Quaternion tilt = Quaternion.AngleAxis(-launchAngle, Vector3.Cross(Vector3.up, flatDir));
            velocity = (tilt * flatDir) * speed;

            damage = attackPower;
            owner = ownerTransform;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            if (isStuck) return;

            velocity += Vector3.down * gravity * Time.deltaTime;

            Vector3 currentPosition = transform.position;
            Vector3 nextPosition = currentPosition + velocity * Time.deltaTime;
            float travelDistance = (nextPosition - currentPosition).magnitude;

            if (Physics.Raycast(currentPosition, velocity.normalized, out RaycastHit hit, travelDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (owner == null || !hit.transform.IsChildOf(owner))
                {
                    transform.position = hit.point;
                    HandleHit(hit.collider);
                    return;
                }
            }

            transform.position = nextPosition;
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        private void HandleHit(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Stick();
        }

        private void Stick()
        {
            isStuck = true;
            velocity = Vector3.zero;
        }
    }
}