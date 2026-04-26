using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public abstract class AIBaseAttackState : AIBaseState
    {
        protected float _attackTimer;
        protected float _outOfSightTimer;
        protected const float OUT_OF_SIGHT_THRESHOLD = 1.5f;

        protected AIBaseAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _attackTimer = 0f;
            _outOfSightTimer = 0f;
        }

        protected bool ShouldExitAttack()
        {
            if (!_stateMachine.Vision.HasTarget)
            {
                _outOfSightTimer += Time.deltaTime;
                return _outOfSightTimer >= OUT_OF_SIGHT_THRESHOLD;
            }
            
            _outOfSightTimer = 0f;
            return false;
        }

        protected void HandleTargetLost()
        {
            if (ShouldExitAttack())
            {
                _stateMachine.SwitchState(AIStateType.Patrol);
            }
        }
    }
}