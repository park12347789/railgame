using UnityEngine;

namespace RailGame.Enemy.Attack
{
    /// <summary>
    /// 적 공격 방식의 공용 규격.
    /// 근접(MeleeAttack), 원거리(RangedAttack) 등이 이걸 구현한다.
    /// 실제 구현체는 별도 브랜치에서 작업. 이번 브랜치에서는 상태머신이 호출할 시그니처만 확정한다.
    /// </summary>
    public interface IEnemyAttack
    {
        /// <summary>
        /// 공격을 시도한다. 쿨다운 등 내부 조건은 구현체가 직접 관리하고,
        /// 실제로 공격이 발동했는지 여부를 반환한다 (애니메이션 트리거 등에 사용 가능).
        /// </summary>
        bool TryAttack(Transform target);
    }
}