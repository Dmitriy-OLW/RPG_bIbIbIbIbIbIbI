/*using UnityEngine;
using Enemy.Data;

namespace Enemy.State
{
    public class AIAggressionState : IAIState
    {
        private AIStateMachine _stateMachine;
        private float _attackDistance;
        private float _desiredDistance;

        public AIAggressionState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            switch (_stateMachine.EnemyType)
            {
                case EnemyType.Melee:
                    _attackDistance = 2f;
                    _desiredDistance = 1.5f;
                    ///_stateMachine.Navigation.SetSpeed(5f); 
                    break;
                    
                case EnemyType.Ranged:
                    _attackDistance = 10f;
                    _desiredDistance = 7f;
                    //.Navigation.SetSpeed(3.5f); 
                    break;
            }
        }

        public void Update()
        {
            if (!_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Search);
                return;
            }

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = Vector3.Distance(_stateMachine.transform.position, target.position);

            if (distanceToTarget <= _attackDistance)
            {
                _stateMachine.SwitchState(AIStateType.Attack);
                return;
            }

            UpdatePosition(target.position, distanceToTarget);
        }

        public void Exit()
        {
            _stateMachine.Navigation.Stop();
        }

        private void UpdatePosition(Vector3 targetPosition, float currentDistance)
        {
            Vector3 directionToTarget = (targetPosition - _stateMachine.transform.position).normalized;
            
            if (_stateMachine.EnemyType == EnemyType.Ranged)
            {
                if (currentDistance < 5f)
                {
                    Vector3 retreatPosition = _stateMachine.transform.position - directionToTarget * 2f;
                    _stateMachine.Navigation.SetDestination(retreatPosition);
                }
                else
                {
                    _stateMachine.Navigation.SetDestination(targetPosition);
                }
            }
            else
            {
                _stateMachine.Navigation.SetDestination(targetPosition);
            }
        }
    }
}*/