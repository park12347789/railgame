namespace RailGame.Enemy.StateMachine
{
    /// <summary>
    /// 적 공용 행동 상태.
    /// 몹 종류(스켈레톤/박쥐/좀비)와 무관하게 공통으로 쓰는 상태 목록.
    /// 몹별 차이는 상태가 아니라 Movement/Attack 구현체로 처리한다.
    /// </summary>
    public enum EnemyStateType
    {
        Idle,
        Chase,
        Attack,
        Death,
    }
}