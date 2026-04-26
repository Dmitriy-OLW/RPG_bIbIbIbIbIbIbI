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

        public override void Update()
        {
            // Проверяем, не нужно ли переключить тип атаки
            _stateMachine.CheckAttackSwitchDuring();
            
            // Проверяем потерю цели
            if (!_stateMachine.Vision.HasTarget)
            {
                _outOfSightTimer += Time.deltaTime;
                
                if (_outOfSightTimer >= OUT_OF_SIGHT_THRESHOLD)
                {
                    _stateMachine.SwitchState(AIStateType.Search);
                }
                return;
            }
            
            _outOfSightTimer = 0f;
        }

        protected bool ShouldReturnToAggression(float distanceToTarget, float aggressionRange)
        {
            return distanceToTarget > aggressionRange;
        }

        public override void Exit()
        {
            _stateMachine.Navigation.ClearPath();
            _stateMachine.InputMapper.SetShouldRun(false);
        }
    }
}