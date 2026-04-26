using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AISearchState : AIBaseState
    {
        private float _searchTimer;
        private bool _hasReachedLastKnownPosition;
        private Vector3 _lastKnownPosition;
        private float _searchPointTimer;
        private Vector3 _currentSearchPoint;
        
        private const float MAX_SEARCH_DURATION = 10f;
        private const float SEARCH_RADIUS = 5f;
        private const float WAYPOINT_REACH_DISTANCE = 2f;
        private const float SEARCH_POINT_INTERVAL = 3f;
        private const float ROTATION_SPEED = 2f;

        public AISearchState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _searchTimer = MAX_SEARCH_DURATION;
            _hasReachedLastKnownPosition = false;
            _searchPointTimer = 0f;
            
            _stateMachine.InputMapper.SetShouldRun(true);

            // Get last known position from vision
            if (_stateMachine.Vision.HasLastKnownPosition && _stateMachine.Vision.IsLastKnownPositionRecent())
            {
                _lastKnownPosition = _stateMachine.Vision.LastKnownPosition;
                
                // Ensure we have a valid position (not zero)
                if (_lastKnownPosition != Vector3.zero)
                {
                    _stateMachine.Navigation.SetDestination(_lastKnownPosition);
                    
                    // Optional: Debug visualization
                    Debug.DrawLine(_stateMachine.transform.position, _lastKnownPosition, Color.yellow, MAX_SEARCH_DURATION);
                    Debug.DrawRay(_lastKnownPosition, Vector3.up * 2f, Color.yellow, MAX_SEARCH_DURATION);
                }
                else
                {
                    // Invalid last known position, return to patrol
                    _stateMachine.Vision.ClearLastKnownPosition();
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
            else
            {
                // No last known position or too old, return to patrol
                _stateMachine.Vision.ClearLastKnownPosition();
                _stateMachine.SwitchState(AIStateType.Patrol);
            }
        }

        public override void Update()
        {
            // If we spot the target again, immediately go to aggression
            if (_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }

            _searchTimer -= Time.deltaTime;
            
            // Check if we've reached the last known position
            if (!_hasReachedLastKnownPosition)
            {
                UpdateMovementToLastKnownPosition();
            }
            else
            {
                // We're at the last known position, perform search pattern
                PerformSearchPattern();
            }
            
            // Time's up, return to patrol
            if (_searchTimer <= 0f)
            {
                _stateMachine.Vision.ClearLastKnownPosition();
                _stateMachine.SwitchState(AIStateType.Patrol);
            }
        }

        private void UpdateMovementToLastKnownPosition()
        {
            float distanceToLastKnown = Vector3.Distance(
                _stateMachine.transform.position, 
                _lastKnownPosition
            );
            
            // Update path if needed
            if (_stateMachine.Navigation.HasReachedDestination && distanceToLastKnown > WAYPOINT_REACH_DISTANCE)
            {
                // Re-attempt to reach last known position if we got stuck
                _stateMachine.Navigation.SetDestination(_lastKnownPosition);
            }
            
            // Check if we've arrived
            if (distanceToLastKnown <= WAYPOINT_REACH_DISTANCE)
            {
                _hasReachedLastKnownPosition = true;
                _stateMachine.Navigation.ClearPath();
                
                // Reduce remaining search time when we arrive
                _searchTimer = Mathf.Min(_searchTimer, 5f);
                
                // Set initial search point
                SetNewSearchPoint();
            }
        }

        private void PerformSearchPattern()
        {
            _searchPointTimer -= Time.deltaTime;
            
            // Look around search area with smooth rotation
            float rotationAngle = Mathf.Sin(Time.time * ROTATION_SPEED) * 45f;
            
            // Apply look direction
            _stateMachine.InputMapper.InputReader.SetLookDirection(
                new Vector2(rotationAngle * 0.5f, 0f)
            );
            
            // Periodically move to new search points
            if (_searchPointTimer <= 0f)
            {
                SetNewSearchPoint();
                _searchPointTimer = SEARCH_POINT_INTERVAL;
            }
            
            // Check if we reached current search point
            if (_stateMachine.Navigation.HasReachedDestination)
            {
                // Small chance to set new search point immediately
                if (Random.value < 0.3f)
                {
                    SetNewSearchPoint();
                }
            }
            
            // Slow down movement while searching
            _stateMachine.InputMapper.SetShouldRun(false);
        }

        private void SetNewSearchPoint()
        {
            // Generate a new search point around the last known position
            Vector2 randomCircle = Random.insideUnitCircle * SEARCH_RADIUS;
            _currentSearchPoint = _lastKnownPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Keep the search point at the same height as last known position
            _currentSearchPoint.y = _lastKnownPosition.y;
            
            // Set destination to new search point
            _stateMachine.Navigation.SetDestination(_currentSearchPoint);
            
            // Visualize search point
            Debug.DrawLine(_stateMachine.transform.position, _currentSearchPoint, Color.cyan, SEARCH_POINT_INTERVAL);
            Debug.DrawRay(_currentSearchPoint, Vector3.up * 1f, Color.cyan, SEARCH_POINT_INTERVAL);
        }

        public override void Exit()
        {
            // Reset input
            _stateMachine.InputMapper.InputReader.SetLookDirection(Vector2.zero);
            _stateMachine.InputMapper.SetShouldRun(false);
            
            // Clear navigation
            _stateMachine.Navigation.ClearPath();
            
            // Clear last known position if we're returning to patrol
            if (_stateMachine.CurrentStateType == AIStateType.Patrol)
            {
                _stateMachine.Vision.ClearLastKnownPosition();
            }
        }
    }
}