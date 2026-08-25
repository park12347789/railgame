using UnityEngine;

namespace RailGame.Enemy.Movement
{
    /// <summary>
    /// 적 이동 방식의 공용 규격.
    /// 지상 이동(GroundMovement), 비행 이동(FlyingMovement) 등이 이걸 구현한다.
    /// 실제 구현체는 별도 브랜치에서 작업. 이번 브랜치에서는 상태머신이 호출할 시그니처만 확정한다.
    /// </summary>
    public interface IEnemyMovement
    {
        /// <summary>목표 지점을 향해 이동을 시작/갱신한다.</summary>
        void MoveTowards(Vector3 destination);

        /// <summary>이동을 멈춘다 (공격 중, 사망 시 등).</summary>
        void Stop();
    }
}