/*using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AISearchState : AIBaseState
    {
        private float _searchTimer;
        private float _searchRadius = 5f;
        private bool _hasReachedLastKnownPosition;

        public AISearchState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _searchTimer = 10f;
            _hasReachedLastKnownPosition = false;
            
            _stateMachine.InputMapper.SetShouldRun(true);

            Vector3 lastKnownPosition = _stateMachine.Vision.LastKnownPosition;
            
            if (lastKnownPosition != Vector3.zero)
            {
                _stateMachine.Navigation.SetDestination(lastKnownPosition);
            }
            else
            {
                ReturnToPatrol();
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
            
            if (!_hasReachedLastKnownPosition && _stateMachine.Navigation.HasReachedDestination)
            {
                _hasReachedLastKnownPosition = true;
                _searchTimer = 5f;
            }
            
            if (_searchTimer <= 0f)
            {
                ReturnToPatrol();
                return;
            }
            
            if (_hasReachedLastKnownPosition)
            {
                LookAround();
            }
        }

        private void LookAround()
        {
            float rotationAngle = Mathf.Sin(Time.time * 2f) * 45f;
            Vector3 lookDirection = Quaternion.Euler(0, rotationAngle, 0) * _stateMachine.transform.forward;
            _stateMachine.InputMapper.InputReader.SetLookDirection(new Vector2(lookDirection.x * 2f, 0f));
        }

        private void ReturnToPatrol()
        {
            _stateMachine.SwitchState(AIStateType.Patrol);
        }

        public override void Exit()
        {
            _stateMachine.InputMapper.InputReader.SetLookDirection(Vector2.zero);
        }
    }
}*/