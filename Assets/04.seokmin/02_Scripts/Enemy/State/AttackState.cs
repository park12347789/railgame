namespace RailGame.Enemy.StateMachine.States
{
    public class AttackState : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Attack;

        public void Enter(EnemyStateContext context)
        {
            context.Movement?.Stop();
        }

        public void Tick(EnemyStateContext context, float deltaTime)
        {
            if (context.Attack != null && context.Attack.IsBusy) return;

            if (context.Target == null)
            {
                context.StateMachine.ChangeState(EnemyStateType.Idle);
                return;
            }

            bool managesOwnEngagement = context.Attack != null && context.Attack.ManagesOwnEngagement;

            if (!managesOwnEngagement && context.DistanceToTarget > context.Stats.AttackRange)
            {
                context.StateMachine.ChangeState(EnemyStateType.Chase);
                return;
            }

            context.Attack?.TryAttack(context.Target);
        }

        public void Exit(EnemyStateContext context) { }
    }
}