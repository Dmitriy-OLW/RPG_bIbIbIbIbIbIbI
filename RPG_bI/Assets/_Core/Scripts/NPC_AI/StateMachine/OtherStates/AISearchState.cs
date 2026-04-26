using UnityEngine;

namespace Enemy.State
{
    public class AISearchState : AIBaseState
    {
        private float _searchTimer;
        private bool _hasReachedLastKnownPosition;
        private Vector3 _lastKnownPosition;
        private float _waitAtPositionTimer;
        private bool _isWaitingAtPosition;
        
        private const float MAX_SEARCH_DURATION = 5f;          
        private const float WAYPOINT_REACH_DISTANCE = 2f;
        private const float WAIT_AT_POSITION_DURATION = 2f;   
        private const float ROTATION_SPEED = 0.1f;              

        public AISearchState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _searchTimer = MAX_SEARCH_DURATION;
            _hasReachedLastKnownPosition = false;
            _isWaitingAtPosition = false;
            _waitAtPositionTimer = 0f;
            
            _stateMachine.InputMapper.SetShouldRun(true);
            
            if (_stateMachine.Vision.HasLastKnownPosition && _stateMachine.Vision.IsLastKnownPositionRecent())
            {
                _lastKnownPosition = _stateMachine.Vision.LastKnownPosition;
                
                if (_lastKnownPosition != Vector3.zero)
                {
                    _stateMachine.Navigation.SetDestination(_lastKnownPosition);
                    
                    Debug.DrawLine(_stateMachine.transform.position, _lastKnownPosition, Color.yellow, MAX_SEARCH_DURATION);
                    Debug.DrawRay(_lastKnownPosition, Vector3.up * 2f, Color.yellow, MAX_SEARCH_DURATION);
                }
                else
                {
                    _stateMachine.Vision.ClearLastKnownPosition();
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
            else
            {
                _stateMachine.Vision.ClearLastKnownPosition();
                _stateMachine.SwitchState(AIStateType.Patrol);
            }
        }

        public override void Update()
        {
            if (_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }

            _searchTimer -= Time.deltaTime;
            
            if (_searchTimer <= 0f)
            {
                _stateMachine.Vision.ClearLastKnownPosition();
                _stateMachine.SwitchState(AIStateType.Patrol);
                return;
            }
            
            if (!_hasReachedLastKnownPosition)
            {
                UpdateMovementToLastKnownPosition();
            }
            else if (_isWaitingAtPosition)
            {
                PerformLookAround();
                
                _waitAtPositionTimer -= Time.deltaTime;
                if (_waitAtPositionTimer <= 0f)
                {
                    _stateMachine.Vision.ClearLastKnownPosition();
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
        }

        private void UpdateMovementToLastKnownPosition()
        {
            float distanceToLastKnown = Vector3.Distance(
                _stateMachine.transform.position, 
                _lastKnownPosition
            );
            
            if (_stateMachine.Navigation.HasReachedDestination && distanceToLastKnown > WAYPOINT_REACH_DISTANCE)
            {
                _stateMachine.Navigation.SetDestination(_lastKnownPosition);
            }
            
            if (distanceToLastKnown <= WAYPOINT_REACH_DISTANCE)
            {
                _hasReachedLastKnownPosition = true;
                _isWaitingAtPosition = true;
                _waitAtPositionTimer = WAIT_AT_POSITION_DURATION;
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
            }
        }

        private void PerformLookAround()
        {
            float rotationAngle = Mathf.Sin(Time.time * ROTATION_SPEED) * 45f;
            
            _stateMachine.InputMapper.InputReader.SetLookDirection(
                new Vector2(rotationAngle * 0.5f, 0f)
            );
        }

        public override void Exit()
        {
            _stateMachine.InputMapper.InputReader.SetLookDirection(Vector2.zero);
            _stateMachine.InputMapper.SetShouldRun(false);
            
            _stateMachine.Navigation.ClearPath();
            
            if (_stateMachine.CurrentStateType == AIStateType.Patrol)
            {
                _stateMachine.Vision.ClearLastKnownPosition();
            }
        }
    }
}