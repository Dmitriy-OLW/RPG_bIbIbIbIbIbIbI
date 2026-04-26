using UnityEngine;

namespace Enemy.State
{
    public class AIFleeState : AIBaseState
    {
        private const float MIN_FLEE_DISTANCE = 10f;
        private const float MAX_FLEE_DISTANCE = 30f;
        private const float FLEE_SPEED_MULTIPLIER = 1.5f;
        private float _fleeDistance;
        private Vector3 _fleeDirection;
        private float _healthRegenCheckTimer;
        private const float HEALTH_REGEN_CHECK_INTERVAL = 1f;

        public AIFleeState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _fleeDistance = Random.Range(MIN_FLEE_DISTANCE, MAX_FLEE_DISTANCE);
            
            if (_stateMachine.Vision.HasTarget)
            {
                // Calculate flee direction away from target
                Vector3 directionFromTarget = (_stateMachine.transform.position - 
                    _stateMachine.Vision.CurrentTarget.position).normalized;
                    
                _fleeDirection = directionFromTarget;
            }
            else
            {
                // Flee in random direction if no target
                _fleeDirection = Random.insideUnitSphere;
                _fleeDirection.y = 0;
                _fleeDirection.Normalize();
            }
            
            Vector3 fleePosition = _stateMachine.transform.position + _fleeDirection * _fleeDistance;
            
            _stateMachine.Navigation.SetDestination(fleePosition);
            _stateMachine.InputMapper.SetShouldRun(true);
            
            _healthRegenCheckTimer = 0f;
        }

        public override void Update()
        {
            _healthRegenCheckTimer += Time.deltaTime;
            
            if (_healthRegenCheckTimer >= HEALTH_REGEN_CHECK_INTERVAL)
            {
                _healthRegenCheckTimer = 0f;
                
                // Update flee position to maintain distance from target
                if (_stateMachine.Vision.HasTarget)
                {
                    float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
                    
                    if (distanceToTarget > _fleeDistance)
                    {
                        // Far enough from target, can stop fleeing
                        _stateMachine.SwitchState(AIStateType.Patrol);
                        return;
                    }
                    
                    // Update flee direction
                    _fleeDirection = (_stateMachine.transform.position - 
                        _stateMachine.Vision.CurrentTarget.position).normalized;
                        
                    Vector3 newFleePosition = _stateMachine.transform.position + _fleeDirection * 5f;
                    _stateMachine.Navigation.SetDestination(newFleePosition);
                }
                else
                {
                    // Lost target, can stop fleeing
                    _stateMachine.SwitchState(AIStateType.Patrol);
                    return;
                }
            }
            
            // Check if reached flee destination
            if (_stateMachine.Navigation.HasReachedDestination)
            {
                if (!_stateMachine.Vision.HasTarget)
                {
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
        }

        public override void Exit()
        {
            _stateMachine.InputMapper.SetShouldRun(false);
            _stateMachine.Navigation.ClearPath();
        }
    }
}