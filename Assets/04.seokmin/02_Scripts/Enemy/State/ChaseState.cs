namespace RailGame.Enemy.StateMachine.States
{
    /// <summary>
    /// 추적 상태. 타겟을 향해 이동하고, 공격 사거리 안에 들어오면 Attack으로 전환한다.
    /// 실제 이동 방식(지상/비행)은 context.Movement 구현체가 처리 — 이 상태는 "언제" 움직일지만 결정한다.
    /// </summary>
    public class ChaseState : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Chase;

        public void Enter(EnemyStateContext context) { }

        public void Tick(EnemyStateContext context, float deltaTime)
        {
            if (context.Target == null)
            {
                context.StateMachine.ChangeState(EnemyStateType.Idle);
                return;
            }

            if (context.DistanceToTarget <= context.Stats.AttackRange)
            {
                context.StateMachine.ChangeState(EnemyStateType.Attack);
                return;
            }

            context.Movement?.MoveTowards(context.Target.position);
        }

        public void Exit(EnemyStateContext context)
        {
            context.Movement?.Stop();
        }
    }
}