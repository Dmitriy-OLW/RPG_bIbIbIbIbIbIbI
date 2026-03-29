using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AIPatrolState : AIBaseState
    {
        private int _currentPatrolIndex;
        private float _waitTimer;
        private bool _isWaiting;

        public AIPatrolState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _currentPatrolIndex = 0;
            _isWaiting = false;
            
            _stateMachine.InputMapper.SetShouldRun(false);
            
            if (_stateMachine.PatrolPoints.Length > 0)
            {
                SetNextPatrolPoint();
            }
        }

        public override void Update()
        {
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    SetNextPatrolPoint();
                }
                return;
            }
            
            if (_stateMachine.Navigation.HasReachedDestination)
            {
                _isWaiting = true;
                _waitTimer = Random.Range(2f, 5f);
                _stateMachine.InputMapper.SetShouldRun(false);
            }
        }

        public override void Exit()
        {
            _stateMachine.Navigation.ClearPath();
        }

        private void SetNextPatrolPoint()
        {
            if (_stateMachine.PatrolPoints.Length == 0)
                return;

            Transform targetPoint = _stateMachine.PatrolPoints[_currentPatrolIndex];
            
            if (targetPoint != null)
            {
                _stateMachine.Navigation.SetDestination(targetPoint.position);
                _stateMachine.InputMapper.SetShouldRun(false);
            }
            
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _stateMachine.PatrolPoints.Length;
        }
    }
}