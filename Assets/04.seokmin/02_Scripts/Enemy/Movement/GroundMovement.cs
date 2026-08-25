using UnityEngine;
using UnityEngine.AI;

namespace RailGame.Enemy.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GroundMovement : MonoBehaviour, IEnemyMovement
    {
        private NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void SetSpeed(float speed)
        {
            agent.speed = speed;
        }

        public void MoveTowards(Vector3 destination)
        {
            if (!agent.isOnNavMesh) return;

            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!agent.isOnNavMesh) return;

            agent.isStopped = true;
        }
    }
}