using RailGame.Enemy.Attack;
using RailGame.Enemy.Movement;
using RailGame.Enemy.Runtime;
using UnityEngine;

namespace RailGame.Enemy.StateMachine
{
    /// <summary>
    /// 상태들이 공유하는 참조 묶음.
    /// 개별 상태 클래스는 상태를 갖지 않고(stateless) 이 컨텍스트를 통해서만 데이터에 접근한다.
    /// → 같은 상태 인스턴스를 여러 적이 공유해도 안전 (플라이웨이트 패턴).
    /// </summary>
    public class EnemyStateContext
    {
        public Transform Self;
        public Transform Target;
        public EnemyRuntimeStats Stats;

        public IEnemyMovement Movement;
        public IEnemyAttack Attack;

        public EnemyStateMachine StateMachine;

        public float DistanceToTarget
        {
            get
            {
                if (Target == null) return float.MaxValue;

                Vector3 selfFlat = new Vector3(Self.position.x, 0f, Self.position.z);
                Vector3 targetFlat = new Vector3(Target.position.x, 0f, Target.position.z);
                return Vector3.Distance(selfFlat, targetFlat);
            }
        }
    }
}