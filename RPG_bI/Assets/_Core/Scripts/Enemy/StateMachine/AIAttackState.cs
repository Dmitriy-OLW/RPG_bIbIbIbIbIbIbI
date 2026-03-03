/*using UnityEngine;
using Enemy.Data;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AIAttackState : IAIState
    {
        private AIStateMachine _stateMachine;
        private float _attackTimer;
        private float _attackCooldown = 1f;

        public AIAttackState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _attackTimer = 0f;
            _stateMachine.Navigation.Stop();
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

            float requiredDistance = _stateMachine.EnemyType == EnemyType.Melee ? 3f : 12f;
            
            if (distanceToTarget > requiredDistance)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }

            _attackTimer -= Time.deltaTime;
            
            if (_attackTimer <= 0f)
            {
                PerformAttack();
                _attackTimer = _attackCooldown;
            }
        }

        public void Exit()
        {
        }

        private void PerformAttack()
        {
            switch (_stateMachine.EnemyType)
            {
                case EnemyType.Melee:
                   // _stateMachine.InputMapper.ResetInput();
                    //.InputMapper.InputReader.PerformPrimaryAttack();
                    break;
                    
                case EnemyType.Ranged:
                   // _stateMachine.InputMapper.ResetInput();
                    //_stateMachine.InputMapper.InputReader.PerformSecondaryAttack();
                    break;
            }
        }
    }
}*/