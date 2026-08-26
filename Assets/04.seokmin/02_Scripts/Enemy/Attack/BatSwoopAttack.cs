using System.Collections;
using RailGame.Enemy.Movement;
using RailGame.Enemy.Temp;
using UnityEngine;

namespace RailGame.Enemy.Attack
{
    public class BatSwoopAttack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private float hoverHeight = 3f;
        [SerializeField] private float hoverSpeed = 5f;
        [SerializeField] private float orbitRadius = 3f;
        [SerializeField] private float orbitSpeed = 90f;
        [SerializeField] private float diveSpeed = 12f;
        [SerializeField] private float retreatSpeed = 8f;
        [SerializeField] private float hitRange = 1.2f;
        [SerializeField] private float arrivalThreshold = 0.15f;
        [SerializeField] private Vector3 diveOffset = Vector3.zero;

        private float attackPower;
        private float attackCooldown;
        private float lastAttackTime = -999f;
        private bool isSwooping;
        private bool isEngaged;
        private Transform currentTarget;
        private float orbitAngle;
        private FlyingMovement flyingMovement;

        public bool IsBusy => isSwooping;
        public bool ManagesOwnEngagement => isEngaged;

        private void Awake()
        {
            flyingMovement = GetComponent<FlyingMovement>();
        }

        public void Initialize(float power, float cooldown)
        {
            attackPower = power;
            attackCooldown = cooldown;
        }

        public bool TryAttack(Transform target)
        {
            currentTarget = target;

            if (!isEngaged)
            {
                isEngaged = true;
                if (flyingMovement != null) flyingMovement.enabled = false;
            }

            if (isSwooping) return false;
            if (Time.time - lastAttackTime < attackCooldown) return false;

            StartCoroutine(SwoopRoutine(target));
            return true;
        }

        private void Update()
        {
            if (isSwooping || currentTarget == null) return;

            orbitAngle += orbitSpeed * Time.deltaTime;
            Vector3 hoverPosition = GetOrbitPosition(currentTarget);

            MoveAndFace(hoverPosition, hoverSpeed);
        }

        private IEnumerator SwoopRoutine(Transform target)
        {
            isSwooping = true;

            Vector3 diveStartPosition = transform.position;
            Vector3 divePosition = target.position + diveOffset;
            yield return MoveTo(divePosition, diveSpeed);

            if (Vector3.Distance(transform.position, target.position) <= hitRange)
            {
                target.GetComponent<IDamageable>()?.TakeDamage(attackPower);
            }

            Vector3 diveDirection = divePosition - diveStartPosition;
            diveDirection.y = 0f;
            if (diveDirection.sqrMagnitude < 0.01f) diveDirection = transform.forward;
            diveDirection.Normalize();

            Vector3 retreatPosition = target.position + diveDirection * orbitRadius + Vector3.up * hoverHeight;
            yield return MoveTo(retreatPosition, retreatSpeed);

            Vector3 finalOffset = retreatPosition - (target.position + Vector3.up * hoverHeight);
            orbitAngle = Mathf.Atan2(finalOffset.z, finalOffset.x) * Mathf.Rad2Deg;

            lastAttackTime = Time.time;
            isSwooping = false;
        }

        private Vector3 GetOrbitPosition(Transform target)
        {
            float radians = orbitAngle * Mathf.Deg2Rad;
            Vector3 orbitOffset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * orbitRadius;
            return target.position + Vector3.up * hoverHeight + orbitOffset;
        }

        private IEnumerator MoveTo(Vector3 destination, float speed)
        {
            while (Vector3.Distance(transform.position, destination) > arrivalThreshold)
            {
                MoveAndFace(destination, speed);
                yield return null;
            }
        }

        private void MoveAndFace(Vector3 destination, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            Vector3 direction = destination - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}