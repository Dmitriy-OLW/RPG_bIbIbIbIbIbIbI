/*using UnityEngine;
using Enemy.Data;

namespace Enemy.State
{
    public class AISearchState : IAIState
    {
        private AIStateMachine _stateMachine;
        private float _searchTimer;
        private float _searchRadius = 5f;

        public AISearchState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _searchTimer = 10f; 
            
            if (_stateMachine.Vision.LastKnownPosition != Vector3.zero)
            {
                _stateMachine.Navigation.SetDestination(_stateMachine.Vision.LastKnownPosition);
                //_stateMachine.Navigation.SetSpeed(4f);
            }
            else
            {
                ReturnToPatrol();
            }
        }

        public void Update()
        {
            if (_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }

            _searchTimer -= Time.deltaTime;

            if (_stateMachine.Navigation.HasReachedDestination)
            {
                if (_searchTimer <= 0f)
                {
                    ReturnToPatrol();
                }
                else
                {
                    SearchNewArea();
                }
            }

            if (_searchTimer <= 0f)
            {
                ReturnToPatrol();
            }
        }

        public void Exit()
        {
        }

        private void SearchNewArea()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _searchRadius;
            Vector3 searchPosition = _stateMachine.Vision.LastKnownPosition + 
                                     new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            _stateMachine.Navigation.SetDestination(searchPosition);
        }

        private void ReturnToPatrol()
        {
            _stateMachine.SwitchState(AIStateType.Patrol);
        }
    }
}*/