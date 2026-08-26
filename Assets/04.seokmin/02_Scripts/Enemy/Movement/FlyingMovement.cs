using UnityEngine;

namespace RailGame.Enemy.Movement
{
    public class FlyingMovement : MonoBehaviour, IEnemyMovement
    {
        [SerializeField] private float flightHeight = 3f;

        private float moveSpeed;
        private Vector3? destination;

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void MoveTowards(Vector3 destinationPosition)
        {
            destination = destinationPosition + Vector3.up * flightHeight;
        }

        public void Stop()
        {
            destination = null;
        }

        private void Update()
        {
            if (destination == null) return;

            transform.position = Vector3.MoveTowards(transform.position, destination.Value, moveSpeed * Time.deltaTime);

            Vector3 direction = destination.Value - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}