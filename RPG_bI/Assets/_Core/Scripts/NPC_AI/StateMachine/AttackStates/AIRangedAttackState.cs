using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;

namespace Enemy.State
{
    public class AIRangedAttackState : AIBaseAttackState
    {
        private bool _isAiming;
        private const float RANGED_AIM_ANGLE = 20f;

        public AIRangedAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _isAiming = false;
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
            
            UpdateRangedBehavior(target, strategyData);
        }

        private void UpdateRangedBehavior(Transform target, StrategyData strategyData)
        {
            Vector3 directionToTarget = (target.position - _stateMachine.transform.position).normalized;
            directionToTarget.y = 0;
            
            float angleToTarget = Vector3.Angle(_stateMachine.transform.forward, directionToTarget);
            _isAiming = angleToTarget <= RANGED_AIM_ANGLE;
            
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _stateMachine.transform.rotation = Quaternion.Slerp(
                    _stateMachine.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 5f
                );
            }
            
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            bool shouldAttack = distanceToTarget <= strategyData.AttackRange;
            
            float preferredDistance = strategyData.PreferredDistance;
            float tooCloseDistance = strategyData.AttackRange * 0.3f; 
            
            if (distanceToTarget < tooCloseDistance)
            {
                Vector3 directionAway = (_stateMachine.transform.position - target.position).normalized;
                Vector3 retreatPosition = _stateMachine.transform.position + directionAway * preferredDistance;
                _stateMachine.Navigation.SetDestination(retreatPosition);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else if (distanceToTarget > preferredDistance && distanceToTarget <= strategyData.AttackRange)
            {
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else if (distanceToTarget > strategyData.AttackRange)
            {
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else
            {
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
            }
            
            if (_isAiming && shouldAttack && _attackTimer <= 0f)
            {
                if (_stateMachine.IsUsingPrimaryAttack)
                {
                    _stateMachine.InputMapper.InputReader.PerformPrimaryAttack();
                }
                else
                {
                    _stateMachine.InputMapper.InputReader.PerformSecondaryAttack();
                }
                
                _stateMachine.OnAttackPerformed();
                
                _attackTimer = strategyData.AttackCooldown;
            }
        }
    }
}