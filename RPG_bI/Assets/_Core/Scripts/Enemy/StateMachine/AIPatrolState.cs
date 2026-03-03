using UnityEngine;

namespace Enemy.State
{
    public class AIPatrolState : IAIState
    {
        private AIStateMachine _stateMachine;
        private int _currentPatrolIndex;
        private float _waitTimer;
        private bool _isWaiting;

        public AIPatrolState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _currentPatrolIndex = 0;
            _isWaiting = false;
            
            if (_stateMachine.PatrolPoints.Length > 0)
            {
                SetNextPatrolPoint();
            }
            
            //_stateMachine.Navigation.SetSpeed(2f); 
        }

        public void Update()
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
                //_stateMachine.InputMapper.ResetInput();
            }
        }

        public void Exit()
        {
            _stateMachine.Navigation.Stop();
        }

        private void SetNextPatrolPoint()
        {
            if (_stateMachine.PatrolPoints.Length == 0)
                return;

            Vector3 targetPoint = _stateMachine.PatrolPoints[_currentPatrolIndex].position;
            _stateMachine.Navigation.SetDestination(targetPoint);
            
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _stateMachine.PatrolPoints.Length;
        }
    }
}