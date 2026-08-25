using System.Collections.Generic;
using UnityEngine;

namespace RailGame.Enemy.StateMachine
{
    /// <summary>
    /// 상태 전환을 관리하는 상태머신.
    /// MonoBehaviour가 아닌 순수 C# 클래스 → Enemy.cs의 Update()에서 Tick()을 호출해서 구동한다.
    /// </summary>
    public class EnemyStateMachine
    {
        private readonly Dictionary<EnemyStateType, IEnemyState> states = new();
        private readonly EnemyStateContext context;
        private IEnemyState currentState;

        public EnemyStateType CurrentStateType => currentState?.StateType ?? EnemyStateType.Idle;

        public EnemyStateMachine(EnemyStateContext context)
        {
            this.context = context;
            context.StateMachine = this;
        }

        public void RegisterState(IEnemyState state)
        {
            states[state.StateType] = state;
        }

        public void Start(EnemyStateType initialState)
        {
            ChangeState(initialState);
        }

        public void ChangeState(EnemyStateType next)
        {
            if (currentState != null && currentState.StateType == next) return;

            if (!states.TryGetValue(next, out var nextState))
            {
                Debug.LogWarning($"[EnemyStateMachine] {next} 상태가 등록되지 않았습니다.");
                return;
            }

            currentState?.Exit(context);
            currentState = nextState;
            currentState.Enter(context);
        }

        public void Tick(float deltaTime)
        {
            currentState?.Tick(context, deltaTime);
        }
    }
}