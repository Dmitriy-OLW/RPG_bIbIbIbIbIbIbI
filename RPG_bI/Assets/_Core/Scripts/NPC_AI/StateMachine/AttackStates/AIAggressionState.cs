using UnityEngine;
using Enemy.Navigation;

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
            var strategyData = _stateMachine.GetCurrentStrategy();
            
            if (strategyData == null) return;
            
            if (distanceToTarget <= strategyData.AttackRange)
            {
                _stateMachine.SwitchState(AIStateType.Attack);
                return;
            }
            
            if (distanceToTarget > strategyData.AggressionRange)
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