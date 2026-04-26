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
            
            StrategyData primaryStrategy = _stateMachine.GetPrimaryStrategy();
            StrategyData secondaryStrategy = _stateMachine.GetSecondaryStrategy();
            
            float primaryAttackRange = primaryStrategy != null ? primaryStrategy.AttackRange : 0f;
            float secondaryAttackRange = secondaryStrategy != null ? secondaryStrategy.AttackRange : 0f;
            
            StrategyData currentStrategy = _stateMachine.GetCurrentStrategy();
            
            if (currentStrategy == null) return;
            
            bool canAttackWithPrimary = distanceToTarget <= primaryAttackRange;
            bool canAttackWithSecondary = distanceToTarget <= secondaryAttackRange;
            
            if (canAttackWithPrimary || canAttackWithSecondary)
            {
                _stateMachine.DetermineAttackTypeForAggression();
                _stateMachine.SwitchState(AIStateType.Attack);
                return;
            }
            
            if (distanceToTarget > currentStrategy.AggressionRange)
            {
                _stateMachine.SwitchState(AIStateType.Search);
                return;
            }
            
            _stateMachine.Navigation.SetDestination(target.position);
        }

        public override void Exit()
        {
        }
    }
}