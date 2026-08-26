namespace RailGame.Enemy.StateMachine.States
{
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

            bool managesOwnEngagement = context.Attack != null && context.Attack.ManagesOwnEngagement;

            if (managesOwnEngagement || context.DistanceToTarget <= context.Stats.AttackRange)
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