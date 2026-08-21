using UnityEngine;

namespace Game.Enemy.Data
{
    /// <summary>
    /// 웨이브 스케일링, 디버프 등으로 변하는 값은 여기 두지 않는다.
    /// 이 SO는 여러 적 인스턴스가 공유하는 읽기 전용 데이터로만 사용한다.
    /// </summary>
 
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Enemy/Enemy Data", order = 0)]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("식별")]
        [Tooltip("몬스터 식별용 ID (스폰 테이블, 저장 데이터 등에서 참조)")]
        public string enemyId;

        [Tooltip("에디터/디버그용 표시 이름")]
        public string displayName;

        [Header("전투 스탯")]
        [Min(1f)]
        public float maxHealth = 10f;

        [Min(0f)]
        public float attackPower = 1f;

        [Min(0f)]
        public float attackRange = 1.5f;

        [Min(0.01f)]
        [Tooltip("공격 1회 후 다음 공격까지 걸리는 시간 (초)")]
        public float attackCooldown = 1f;

        [Header("이동")]
        [Min(0f)]
        public float moveSpeed = 3.5f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                enemyId = name;
            }
        }
#endif
    }
}
