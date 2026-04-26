/*using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;
using Weapons;

namespace Enemy.State
{
    public class AIAttackState : AIBaseState
    {
        private float _attackTimer;
        private float _sideStepTimer;
        private int _sideStepDirection = 1;
        
        private const float MELEE_HOLD_DISTANCE = 3f;
        private const float OUT_OF_SIGHT_THRESHOLD = 1.5f;
        private const float RANGED_AIM_ANGLE = 20f;
        
        private float _outOfSightTimer;
        private bool _isAiming;

        public AIAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _attackTimer = 0f;
            _sideStepTimer = 0f;
            _outOfSightTimer = 0f;
            _isAiming = false;
        }

        public override void Update()
        {
            if (!_stateMachine.Vision.HasTarget)
            {
                _outOfSightTimer += Time.deltaTime;
                
                if (_outOfSightTimer >= OUT_OF_SIGHT_THRESHOLD)
                {
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
                return;
            }

            _outOfSightTimer = 0f;

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            var strategy = _stateMachine.BehaviorStrategy;
            
            _attackTimer -= Time.deltaTime;

            bool shouldExitAttack = CheckExitConditions(target, distanceToTarget, strategy);
            
            if (shouldExitAttack)
            {
                if (distanceToTarget > strategy.AggressionRange)
                {
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
                else
                {
                    _stateMachine.SwitchState(AIStateType.Aggression);
                }
                return;
            }
            
            if (_stateMachine.EnemyType == EnemyType.Ranged)
            {
                UpdateRangedAttack(target, strategy);
            }
            else
            {
                UpdateMeleeAttack(target, strategy);
            }
        }

        private void UpdateRangedAttack(Transform target, IEnemyBehaviorStrategy strategy)
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
            
            strategy.UpdatePosition(_stateMachine.transform, target, out bool shouldAttack);
            
            float preferredDistance = strategy.PreferredDistance;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            
            if (distanceToTarget < 5f)
            {
                Vector3 directionAway = (_stateMachine.transform.position - target.position).normalized;
                Vector3 retreatPosition = _stateMachine.transform.position + directionAway * 7f;
                _stateMachine.Navigation.SetDestination(retreatPosition);
                _stateMachine.InputMapper.SetShouldRun(true);
                _isAiming = false;
            }
            else if (distanceToTarget > preferredDistance && distanceToTarget <= 10f)
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
                strategy.PerformAttack(_stateMachine.InputMapper.InputReader);
                _attackTimer = strategy.AttackCooldown;
            }
        }

        private void UpdateMeleeAttack(Transform target, IEnemyBehaviorStrategy strategy)
        {
            strategy.UpdatePosition(_stateMachine.transform, target, out bool shouldAttack);
            
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            
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
                strategy.PerformAttack(_stateMachine.InputMapper.InputReader);
                _attackTimer = strategy.AttackCooldown;
            }
        }

        private bool CheckExitConditions(Transform target, float distance, IEnemyBehaviorStrategy strategy)
        {
            switch (_stateMachine.EnemyType)
            {
                case EnemyType.Melee:
                    return distance > MELEE_HOLD_DISTANCE;
                    
                case EnemyType.Ranged:
                    return !_stateMachine.Vision.HasTarget;
                    
                default:
                    return distance > strategy.AttackRange;
            }
        }

        public override void Exit()
        {
            _stateMachine.Navigation.ClearPath();
            _stateMachine.InputMapper.SetShouldRun(false);
        }
    }
}*/