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

        // 실제 구현체는 다음 브랜치에서 GetComponent로 채워짐. 지금은 null일 수 있으므로
        // 각 상태에서 반드시 null 체크 후 사용한다.
        public IEnemyMovement Movement;
        public IEnemyAttack Attack;

        public EnemyStateMachine StateMachine;

        public float DistanceToTarget =>
            Target == null ? float.MaxValue : Vector3.Distance(Self.position, Target.position);
    }
}