namespace RailGame.Enemy.StateMachine.States
{
    /// <summary>
    /// 사망 상태. Stats.OnDied 이벤트 발생 시 어디서든 강제 진입한다 (Enemy.cs에서 트리거).
    /// 실제 사망 처리(이벤트 발행, 보상 드랍, 오브젝트 풀 반환 등)는 Events 브랜치에서 연결 예정.
    /// </summary>
    public class DeathState : IEnemyState
    {
        public EnemyStateType StateType => EnemyStateType.Death;

        public void Enter(EnemyStateContext context)
        {
            context.Movement?.Stop();

            // TODO(Events 브랜치): 여기서 EnemyDiedEvent(GameEventSO) Raise()
            // TODO: 콜라이더 비활성화, 사망 애니메이션 트리거, 오브젝트 풀 반환 등
        }

        public void Tick(EnemyStateContext context, float deltaTime) { }

        public void Exit(EnemyStateContext context) { }
    }
}