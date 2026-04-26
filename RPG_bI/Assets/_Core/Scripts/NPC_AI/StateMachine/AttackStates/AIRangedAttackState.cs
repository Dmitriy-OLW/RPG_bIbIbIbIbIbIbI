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
            if (!_stateMachine.Vision.HasTarget)
            {
                HandleTargetLost();
                return;
            }

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            var strategyData = _stateMachine.GetCurrentStrategy();;
            
            _attackTimer -= Time.deltaTime;

            if (distanceToTarget > strategyData.AggressionRange)
            {
                _stateMachine.SwitchState(AIStateType.Patrol);
                return;
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
            bool shouldAttack = distanceToTarget <= strategyData.AttackRange && distanceToTarget >= 5f;
            
            if (distanceToTarget < 5f)
            {
                Vector3 directionAway = (_stateMachine.transform.position - target.position).normalized;
                Vector3 retreatPosition = _stateMachine.transform.position + directionAway * 7f;
                _stateMachine.Navigation.SetDestination(retreatPosition);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else if (distanceToTarget > strategyData.PreferredDistance && distanceToTarget <= 10f)
            {
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else if (distanceToTarget > 10f)
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
                _stateMachine.InputMapper.InputReader.PerformSecondaryAttack();
                _attackTimer = strategyData.AttackCooldown;
            }
        }

        public override void Exit()
        {
            _stateMachine.Navigation.ClearPath();
            _stateMachine.InputMapper.SetShouldRun(false);
        }
    }
}