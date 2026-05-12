using UnityEngine;
using System.Collections.Generic;

namespace Enemy.State
{
    public class AIFleeState : AIBaseState
    {
        private const float MIN_FLEE_DISTANCE = 10f;
        private const float MAX_FLEE_DISTANCE = 30f;
        private const float MIN_WAIT_TIME = 5f;
        private const float MAX_WAIT_TIME = 10f;
        private const float HEAL_PERCENT_PER_SECOND = 0.1f; 
        
        private Vector3 _fleeDestination;
        private bool _hasReachedDestination;
        private float _waitTimer;
        private bool _isWaiting;

        public AIFleeState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _hasReachedDestination = false;
            _isWaiting = false;
            _waitTimer = 0f;
            
            _fleeDestination = GetRandomFleePoint();
            
            if (_fleeDestination == Vector3.zero && _stateMachine.Vision.HasTarget)
            {
                Vector3 directionFromTarget = (_stateMachine.transform.position - 
                    _stateMachine.Vision.CurrentTarget.position).normalized;
                    
                float fleeDistance = Random.Range(MIN_FLEE_DISTANCE, MAX_FLEE_DISTANCE);
                _fleeDestination = _stateMachine.transform.position + directionFromTarget * fleeDistance;
            }
            else if (_fleeDestination == Vector3.zero)
            {
                _stateMachine.SwitchState(AIStateType.Patrol);
                return;
            }
            
            _stateMachine.Navigation.SetDestination(_fleeDestination);
            _stateMachine.InputMapper.SetShouldRun(true);
        }

        public override void Update()
        {
            if (!_hasReachedDestination && _stateMachine.Vision.HasTarget)
            {
                float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
                
                if (distanceToTarget < MIN_FLEE_DISTANCE)
                {
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                    }
                    else
                    {
                        Vector3 directionFromTarget = (_stateMachine.transform.position - 
                                                       _stateMachine.Vision.CurrentTarget.position).normalized;
                            
                        _fleeDestination = _stateMachine.transform.position + directionFromTarget * MIN_FLEE_DISTANCE;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                    }
                }
            }

            if (!_hasReachedDestination && _stateMachine.Navigation.HasReachedDestination)
            {
                _hasReachedDestination = true;
                _isWaiting = true;
                _waitTimer = Random.Range(MIN_WAIT_TIME, MAX_WAIT_TIME);
                
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
                
                if (_stateMachine.Vision.HasTarget)
                {
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _hasReachedDestination = false;
                        _isWaiting = false;
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                        _stateMachine.InputMapper.SetShouldRun(true);
                    }
                }
            }
            
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                
                if (_stateMachine.CanSelfHeal && _stateMachine.HealthController != null)
                {
                    _stateMachine.HealthController.HealPercent(HEAL_PERCENT_PER_SECOND * Time.deltaTime);
                }
                
                if (_stateMachine.Vision.HasTarget)
                {
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _isWaiting = false;
                        _hasReachedDestination = false;
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                        _stateMachine.InputMapper.SetShouldRun(true);
                        return;
                    }
                }
                
                if (_waitTimer <= 0f)
                {
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
        }
        
        private Vector3 GetRandomFleePoint()
        {
            if (_stateMachine.PatrolPoints == null || _stateMachine.PatrolPoints.Length == 0)
                return Vector3.zero;

            List<Transform> availablePoints = new List<Transform>();
            
            foreach (var point in _stateMachine.PatrolPoints)
            {
                if (point != null)
                {
                    availablePoints.Add(point);
                }
            }
            
            if (availablePoints.Count == 0)
                return Vector3.zero;
            
            if (_stateMachine.Vision.HasTarget)
            {
                Vector3 targetPos = _stateMachine.Vision.CurrentTarget.position;
                
                availablePoints.Sort((a, b) => 
                    Vector3.Distance(b.position, targetPos)
                    .CompareTo(Vector3.Distance(a.position, targetPos)));
                
                int count = Mathf.Max(1, availablePoints.Count / 2);
                int randomIndex = Random.Range(0, count);
                return availablePoints[randomIndex].position;
            }
            
            return availablePoints[Random.Range(0, availablePoints.Count)].position;
        }

        public override void Exit()
        {
            _stateMachine.InputMapper.SetShouldRun(false);
            _stateMachine.Navigation.ClearPath();
        }
    }
}