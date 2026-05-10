using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AISearchState : AIBaseState
    {
        private const float DIRECT_TRACK_DURATION = 3f;
        
        private const float FIRST_TURN_DURATION = 2f;  
        private const float SECOND_TURN_DURATION = 4f; 
        private const float TURN_ANGLE_FIRST = 90f;
        private const float TURN_ANGLE_SECOND = 180f;
        
        private const float WAYPOINT_REACH_DISTANCE = 2f;
        
        private enum SearchPhase
        {
            DirectTracking,    
            MovingToLastPos,  
            FirstTurn,        
            SecondTurn         
        }
        
        private SearchPhase _currentPhase;
        private float _phaseTimer;
        
        private Transform _trackedTarget;
        
        private Vector3 _targetPosition;
        
        private float _firstTurnDirection;  
        private float _secondTurnDirection; 

        private Quaternion _turnStartRotation;
        private Quaternion _turnTargetRotation;

        public AISearchState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
    
            _stateMachine.InputMapper.SetShouldRun(true);
            
            _trackedTarget = _stateMachine.Vision.LastSeenTarget;
            
            if (_trackedTarget != null)
            {
                _currentPhase = SearchPhase.DirectTracking;
                _phaseTimer = DIRECT_TRACK_DURATION;
                
                _stateMachine.Navigation.SetDestination(_trackedTarget.position);
                
            }
            else if (_stateMachine.Vision.HasLastKnownPosition && _stateMachine.Vision.IsLastKnownPositionRecent())
            {
                _targetPosition = _stateMachine.Vision.LastKnownPosition;
        
                if (_targetPosition != Vector3.zero)
                {
                    _currentPhase = SearchPhase.MovingToLastPos;
                    _phaseTimer = 0f;
            
                    _stateMachine.Navigation.SetDestination(_targetPosition);
                }
                else
                {
                    ReturnToPatrol();
                    return;
                }
            }
            else
            {
                ReturnToPatrol();
                return;
            }
        }

        public override void Update()
        {
            if (_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }
            
            switch (_currentPhase)
            {
                case SearchPhase.DirectTracking:
                    UpdateDirectTracking();
                    break;
                    
                case SearchPhase.MovingToLastPos:
                    UpdateMovingToLastPos();
                    break;
                    
                case SearchPhase.FirstTurn:
                    UpdateFirstTurn();
                    break;
                    
                case SearchPhase.SecondTurn:
                    UpdateSecondTurn();
                    break;
            }
        }
        
        private void UpdateDirectTracking()
        {
            _phaseTimer -= Time.deltaTime;
            
            if (_trackedTarget != null)
            {
                _stateMachine.Navigation.SetDestination(_trackedTarget.position);
                _stateMachine.InputMapper.SetShouldRun(true);
            }
            
            if (_phaseTimer <= 0f)
            {
                if (_trackedTarget != null)
                {
                    _targetPosition = _trackedTarget.position;
                }
                else if (_stateMachine.Vision.HasLastKnownPosition)
                {
                    _targetPosition = _stateMachine.Vision.LastKnownPosition;
                }
                else
                {
                    ReturnToPatrol();
                    return;
                }
                
                _trackedTarget = null;

                _currentPhase = SearchPhase.MovingToLastPos;
                _stateMachine.Navigation.SetDestination(_targetPosition);
                
                Debug.DrawLine(_stateMachine.transform.position, _targetPosition, Color.cyan, 5f);
            }
        }
        
        private void UpdateMovingToLastPos()
        {
            float distanceToTarget = Vector3.Distance(
                _stateMachine.transform.position, 
                _targetPosition
            );
            
            if (_stateMachine.Navigation.HasReachedDestination && distanceToTarget > WAYPOINT_REACH_DISTANCE)
            {
                _stateMachine.Navigation.SetDestination(_targetPosition);
            }
            
            _stateMachine.InputMapper.SetShouldRun(true);
            
            if (distanceToTarget <= WAYPOINT_REACH_DISTANCE)
            {
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
                
                StartFirstTurn();
            }
        }
        
        private void StartFirstTurn()
        {
            _currentPhase = SearchPhase.FirstTurn;
            _phaseTimer = FIRST_TURN_DURATION;
            
            _firstTurnDirection = Random.value > 0.5f ? 1f : -1f;
            
            _turnStartRotation = _stateMachine.transform.rotation;
            float targetAngle = _stateMachine.transform.eulerAngles.y + (TURN_ANGLE_FIRST * _firstTurnDirection);
            _turnTargetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        
        private void UpdateFirstTurn()
        {
            _phaseTimer -= Time.deltaTime;
            
            float progress = 1f - (_phaseTimer / FIRST_TURN_DURATION);
            _stateMachine.transform.rotation = Quaternion.Slerp(
                _turnStartRotation, 
                _turnTargetRotation, 
                progress
            );
            
            if (_phaseTimer <= 0f)
            {
                StartSecondTurn();
            }
        }
        
        private void StartSecondTurn()
        {
            _currentPhase = SearchPhase.SecondTurn;
            _phaseTimer = SECOND_TURN_DURATION;
            
            _secondTurnDirection = -_firstTurnDirection;
            
            _turnStartRotation = _stateMachine.transform.rotation;
            float targetAngle = _stateMachine.transform.eulerAngles.y + (TURN_ANGLE_SECOND * _secondTurnDirection);
            _turnTargetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        
        private void UpdateSecondTurn()
        {
            _phaseTimer -= Time.deltaTime;
            
            float progress = 1f - (_phaseTimer / SECOND_TURN_DURATION);
            _stateMachine.transform.rotation = Quaternion.Slerp(
                _turnStartRotation, 
                _turnTargetRotation, 
                progress
            );
            
            if (_phaseTimer <= 0f)
            {
                ReturnToPatrol();
            }
        }
        
        private void ReturnToPatrol()
        {
            _stateMachine.Vision.ClearLastKnownPosition();
            _stateMachine.Vision.ClearLastSeenTarget();
            _stateMachine.SwitchState(AIStateType.Patrol);
        }

        public override void Exit()
        {
            _trackedTarget = null;
            
            _stateMachine.InputMapper.SetShouldRun(false);
            _stateMachine.Navigation.ClearPath();
            
            _stateMachine.InputMapper.InputReader.SetLookDirection(Vector2.zero);
        }
    }
}