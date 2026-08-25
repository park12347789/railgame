using RailGame.Enemy.Data;
using RailGame.Enemy.Runtime;
using UnityEngine;

namespace RailGame.Enemy
{
    /// <summary>
    /// 적 오브젝트의 진입점.
    /// EnemyDataSO(기본값)를 받아서 스폰 시 개별 EnemyRuntimeStats를 생성한다.
    /// 이동/공격/상태머신 등은 아직 미구현 — 다음 브랜치에서 이 컴포넌트를 참조해서 확장 예정.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyDataSO enemyData;

        public EnemyRuntimeStats Stats { get; private set; }

        private void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError($"{name}: EnemyDataSO가 할당되지 않았습니다.", this);
                return;
            }

            Initialize(enemyData, waveMultiplier: 1f);
        }

        /// <summary>
        /// 스포너에서 웨이브 배수를 넘겨 초기화할 때 사용.
        /// </summary>
        public void Initialize(EnemyDataSO data, float waveMultiplier)
        {
            enemyData = data;
            Stats = new EnemyRuntimeStats(data, waveMultiplier);

            Stats.OnDied += HandleDeath;
        }

        public void TakeDamage(float amount)
        {
            Stats?.TakeDamage(amount);
        }

        private void HandleDeath()
        {
            // TODO: 사망 애니메이션, 보상 드랍, 오브젝트 풀 반환 등
            Debug.Log($"{enemyData.displayName} 사망");
        }

        private void OnDestroy()
        {
            if (Stats != null)
            {
                Stats.OnDied -= HandleDeath;
            }
        }
    }
}