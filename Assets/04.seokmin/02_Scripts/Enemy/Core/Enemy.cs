using RailGame.Enemy.Temp;
using RailGame.Enemy.Attack;
using RailGame.Enemy.Data;
using RailGame.Enemy.Movement;
using RailGame.Enemy.Runtime;
using RailGame.Enemy.StateMachine;
using RailGame.Enemy.StateMachine.States;
using UnityEngine;

namespace RailGame.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDataSO enemyData;

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

            if (stateContext != null)
            {
                stateContext.Target = target;
            }
        }

        private void Update()
        {
            StateMachineInstance?.Tick(Time.deltaTime);
        }

        public void Initialize(EnemyDataSO data, float waveMultiplier)
        {
            enemyData = data;
            Stats = new EnemyRuntimeStats(data, waveMultiplier);
            Stats.OnDied += HandleDeath;

            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            var movement = GetComponent<IEnemyMovement>();
            var attack = GetComponent<IEnemyAttack>();

            movement?.SetSpeed(Stats.MoveSpeed);
            attack?.Initialize(Stats.AttackPower, Stats.AttackCooldown);

            stateContext = new EnemyStateContext
            {
                Self = transform,
                Target = target,
                Stats = Stats,
                Movement = movement,
                Attack = attack,
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