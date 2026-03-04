using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;

namespace Enemy.State
{
    public class AIAttackState : AIBaseState
    {
        private float _attackTimer;
        private Vector3 _lastTargetPosition;
        private float _sideStepTimer;
        private int _sideStepDirection = 1;

        public AIAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _attackTimer = 0f;
            _sideStepTimer = 0f;
            
            var strategy = _stateMachine.BehaviorStrategy;
            _stateMachine.InputMapper.SetShouldRun(false);
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
            var strategy = _stateMachine.BehaviorStrategy;
            
            _attackTimer -= Time.deltaTime;

            if (distanceToTarget > strategy.AttackRange)
            {
                if (distanceToTarget > strategy.AggressionRange)
                {
                    _stateMachine.SwitchState(AIStateType.Search);
                }
                else
                {
                    _stateMachine.SwitchState(AIStateType.Aggression);
                }
                return;
            }
            
            strategy.UpdatePosition(_stateMachine.transform, target, out bool shouldAttack);
            
            UpdateMovementForType(target, distanceToTarget, strategy);
            
            if (shouldAttack && _attackTimer <= 0f)
            {
                strategy.PerformAttack(_stateMachine.InputMapper.InputReader);
                _attackTimer = strategy.AttackCooldown;
            }
        }

        private void UpdateMovementForType(Transform target, float distance, IEnemyBehaviorStrategy strategy)
        {
            if (_stateMachine.EnemyType == EnemyType.Melee)
            {
                _stateMachine.Navigation.SetDestination(target.position);
            }
            else 
            {
                float preferredDistance = strategy.PreferredDistance;
                
                if (distance < 5f) 
                {
                    Vector3 directionAway = (_stateMachine.transform.position - target.position).normalized;
                    Vector3 retreatPosition = _stateMachine.transform.position + directionAway * 2f;
                    _stateMachine.Navigation.SetDestination(retreatPosition);
                }
                else if (distance > preferredDistance) // Слишком далеко - подходим
                {
                    _stateMachine.Navigation.SetDestination(target.position);
                }
                else // На хорошей дистанции - можем стрейфиться
                {
                    UpdateSideStep(target);
                }
            }
        }

        private void UpdateSideStep(Transform target)
        {
            _sideStepTimer -= Time.deltaTime;
            
            if (_sideStepTimer <= 0f)
            {
                _sideStepDirection *= -1; // Меняем направление
                _sideStepTimer = Random.Range(2f, 4f);
            }
            
            // Двигаемся вбок относительно цели
            Vector3 right = Vector3.Cross(Vector3.up, (target.position - _stateMachine.transform.position).normalized);
            Vector3 sideStepPosition = _stateMachine.transform.position + right * _sideStepDirection * 2f;
            
            _stateMachine.Navigation.SetDestination(sideStepPosition);
        }

        public override void Exit()
        {
            _stateMachine.Navigation.ClearPath();
        }
    }
}