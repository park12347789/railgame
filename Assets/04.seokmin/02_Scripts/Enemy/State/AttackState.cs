namespace RailGame.Enemy.StateMachine.States
{
    /// <summary>
    /// 공격 상태. 타겟이 사거리 안에 있는 동안 공격을 시도한다.
    /// 쿨다운 판정은 context.Attack 구현체 내부 책임 (이 상태는 매 프레임 TryAttack만 호출).
    /// 근접/원거리 차이는 IEnemyAttack 구현체가 다를 뿐, 이 상태는 동일하게 동작한다.
    /// </summary>
    public class AttackState : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Attack;

        public void Enter(EnemyStateContext context)
        {
            context.Movement?.Stop();
        }

        public void Tick(EnemyStateContext context, float deltaTime)
        {
            if (context.Target == null)
            {
                context.StateMachine.ChangeState(EnemyStateType.Idle);
                return;
            }

            if (context.DistanceToTarget > context.Stats.AttackRange)
            {
                context.StateMachine.ChangeState(EnemyStateType.Chase);
                return;
            }

            context.Attack?.TryAttack(context.Target);
        }

        public void Exit(EnemyStateContext context) { }
    }
}