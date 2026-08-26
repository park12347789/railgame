using UnityEngine;
using UnityEngine.AI;

namespace RailGame.Enemy.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GroundMovement : MonoBehaviour, IEnemyMovement
    {
        private NavMeshAgent agent;

        private NavMeshAgent Agent => agent != null ? agent : (agent = GetComponent<NavMeshAgent>());

        public void SetSpeed(float speed)
        {
            Agent.speed = speed;
        }

        public void MoveTowards(Vector3 destination)
        {
            if (!Agent.isOnNavMesh) return;

            Agent.isStopped = false;
            Agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!Agent.isOnNavMesh) return;

            Agent.isStopped = true;
        }
    }
}