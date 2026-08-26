namespace RailGame.Enemy.StateMachine
{
    /// <summary>
    /// 개별 행동 상태의 공용 규격.
    /// 상태 자체는 데이터를 갖지 않고(stateless), 필요한 데이터는 전부 EnemyStateContext를 통해 받는다.
    /// </summary>
    public interface IEnemyState
    {
        EnemyStateType StateType { get; }

        /// <summary>이 상태로 처음 진입할 때 1회 호출</summary>
        void Enter(EnemyStateContext context);

        /// <summary>이 상태에 머무는 동안 매 프레임 호출</summary>
        void Tick(EnemyStateContext context, float deltaTime);

        /// <summary>다른 상태로 빠져나갈 때 1회 호출</summary>
        void Exit(EnemyStateContext context);
    }
}