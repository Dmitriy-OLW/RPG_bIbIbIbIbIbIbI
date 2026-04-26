using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;

namespace Enemy.State
{
    public class AIMeleeAttackState : AIBaseAttackState
    {
        private float _sideStepTimer;
        private int _sideStepDirection = 1;
        private const float MELEE_HOLD_DISTANCE = 3f;

        public AIMeleeAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _sideStepTimer = 0f;
        }

        public override void Update()
        {
            base.Update();
            
            if (!_stateMachine.Vision.HasTarget)
                return;

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            
            var strategyData = _stateMachine.GetCurrentStrategy();
            
            if (strategyData == null) return;
            
            _attackTimer -= Time.deltaTime;
            
            if (distanceToTarget > strategyData.AttackRange)
            {
                StrategyData otherStrategy = _stateMachine.IsUsingPrimaryAttack 
                    ? _stateMachine.GetSecondaryStrategy() 
                    : _stateMachine.GetPrimaryStrategy();
                
                if (otherStrategy != null && distanceToTarget <= otherStrategy.AttackRange)
                {
                    _stateMachine.CheckAttackSwitchDuring();
                    return;
                }

                if (distanceToTarget > strategyData.AggressionRange)
                {
                    _stateMachine.SwitchState(AIStateType.Aggression);
                    return;
                }
            }
            
            UpdateMeleeBehavior(target, strategyData);
        }

        private void UpdateMeleeBehavior(Transform target, StrategyData strategyData)
        {
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            bool shouldAttack = distanceToTarget <= strategyData.AttackRange;
            
            if (distanceToTarget > MELEE_HOLD_DISTANCE)
            {
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
            }
            else
            {
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(false);
            }
            
            if (shouldAttack && _attackTimer <= 0f)
            {
                if (_stateMachine.IsUsingPrimaryAttack)
                {
                    _stateMachine.InputMapper.InputReader.PerformPrimaryAttack();
                }
                else
                {
                    _stateMachine.InputMapper.InputReader.PerformSecondaryAttack();
                }
                
                _attackTimer = strategyData.AttackCooldown;
            }
        }
    }
}