using UnityEngine;
using Enemy.Strategies;
using Weapons;

namespace Enemy.State
{
    public class AIAggressionState : AIBaseState
    {
        public AIAggressionState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _stateMachine.InputMapper.SetShouldRun(true);
        }

        public override void Update()
        {
            if (!_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Search);
                return;
            }

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            
            // Получаем дистанции атаки для primary и secondary
            StrategyData primaryStrategy = _stateMachine.GetPrimaryStrategy();
            StrategyData secondaryStrategy = _stateMachine.GetSecondaryStrategy();
            
            float primaryAttackRange = primaryStrategy != null ? primaryStrategy.AttackRange : 0f;
            float secondaryAttackRange = secondaryStrategy != null ? secondaryStrategy.AttackRange : 0f;
            
            // Получаем стратегию для определения aggression range
            // Используем primary стратегию как основную для аггрессии
            StrategyData currentStrategy = _stateMachine.GetCurrentStrategy();
            
            if (currentStrategy == null) return;
            
            // Проверяем, может ли враг атаковать хоть чем-то
            bool canAttackWithPrimary = distanceToTarget <= primaryAttackRange;
            bool canAttackWithSecondary = distanceToTarget <= secondaryAttackRange;
            
            // Если можем атаковать любым оружием - переходим в атаку
            if (canAttackWithPrimary || canAttackWithSecondary)
            {
                // Определяем тип атаки перед переходом
                _stateMachine.DetermineAttackTypeForAggression();
                _stateMachine.SwitchState(AIStateType.Attack);
                return;
            }
            
            // Если вне зоны агрессии - уходим в поиск
            if (distanceToTarget > currentStrategy.AggressionRange)
            {
                _stateMachine.SwitchState(AIStateType.Search);
                return;
            }

            // Продолжаем движение к цели
            _stateMachine.Navigation.SetDestination(target.position);
        }

        public override void Exit()
        {
        }
    }
}