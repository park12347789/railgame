namespace RailGame.Enemy.StateMachine.States
{
    /// <summary>
    /// 대기 상태. 타겟이 DetectionRange 안에 들어오면 Chase로 전환한다.
    /// 상태 자체는 데이터를 갖지 않으므로 모든 적이 같은 인스턴스를 공유해도 된다.
    /// </summary>
    public class IdleState : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Idle;

        public void Enter(EnemyStateContext context)
        {
            context.Movement?.Stop();
        }

        public void Tick(EnemyStateContext context, float deltaTime)
        {
            if (context.Target == null) return;

            if (context.DistanceToTarget <= context.Stats.DetectionRange)
            {
                context.StateMachine.ChangeState(EnemyStateType.Chase);
            }
        }

        public void Exit(EnemyStateContext context) { }
    }
}