using RailGame.Enemy.Attack;
using RailGame.Enemy.Data;
using RailGame.Enemy.Movement;
using RailGame.Enemy.Runtime;
using RailGame.Enemy.StateMachine;
using RailGame.Enemy.StateMachine.States;
using UnityEngine;

namespace RailGame.Enemy
{
    /// <summary>
    /// 적 오브젝트의 진입점.
    /// EnemyDataSO(기본값)를 받아서 스폰 시 개별 EnemyRuntimeStats를 생성하고,
    /// 공용 상태머신(Idle/Chase/Attack/Death)을 구동한다.
    /// 실제 이동/공격 동작은 같은 오브젝트의 IEnemyMovement/IEnemyAttack 구현체에 위임한다.
    /// (구현체가 아직 없으면 null로 남고, 상태들이 null 체크 후 스킵한다 — 다음 브랜치에서 채워짐)
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyDataSO enemyData;

        [Tooltip("비워두면 Start 시 'Player' 태그로 자동 탐색. 추후 별도 타겟팅 시스템으로 교체 예정.")]
        [SerializeField] private Transform target;

        public EnemyRuntimeStats Stats { get; private set; }
        public EnemyStateMachine StateMachineInstance { get; private set; }

        private EnemyStateContext stateContext;

        private void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError($"{name}: EnemyDataSO가 할당되지 않았습니다.", this);
                return;
            }

            Initialize(enemyData, waveMultiplier: 1f);
        }

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }

            // Awake 시점엔 target이 비어있을 수 있으므로 확정된 시점에 컨텍스트에 반영
            if (stateContext != null)
            {
                stateContext.Target = target;
            }
        }

        private void Update()
        {
            StateMachineInstance?.Tick(Time.deltaTime);
        }

        /// <summary>
        /// 스포너에서 웨이브 배수를 넘겨 초기화할 때 사용.
        /// </summary>
        public void Initialize(EnemyDataSO data, float waveMultiplier)
        {
            enemyData = data;
            Stats = new EnemyRuntimeStats(data, waveMultiplier);
            Stats.OnDied += HandleDeath;

            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            stateContext = new EnemyStateContext
            {
                Self = transform,
                Target = target,
                Stats = Stats,
                Movement = GetComponent<IEnemyMovement>(), // 미구현 시 null
                Attack = GetComponent<IEnemyAttack>(),     // 미구현 시 null
            };

            StateMachineInstance = new EnemyStateMachine(stateContext);
            StateMachineInstance.RegisterState(new IdleState());
            StateMachineInstance.RegisterState(new ChaseState());
            StateMachineInstance.RegisterState(new AttackState());
            StateMachineInstance.RegisterState(new DeathState());
            StateMachineInstance.Start(EnemyStateType.Idle);
        }

        public void TakeDamage(float amount)
        {
            Stats?.TakeDamage(amount);
        }

        private void HandleDeath()
        {
            StateMachineInstance?.ChangeState(EnemyStateType.Death);
            // TODO(Events 브랜치): 사망 이벤트 발행, 보상 드랍, 오브젝트 풀 반환 등
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