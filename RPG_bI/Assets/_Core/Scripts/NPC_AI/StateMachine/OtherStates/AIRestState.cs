using UnityEngine;

namespace Enemy.State
{
    public class AIRestState : AIBaseState
    {
        private float _restDuration;
        private float _restTimer;
        private const float MIN_REST_TIME = 1f;
        private const float MAX_REST_TIME = 3f;

        public AIRestState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _restDuration = Random.Range(MIN_REST_TIME, MAX_REST_TIME);
            _restTimer = 0f;
            
            _stateMachine.Navigation.Stop();
            _stateMachine.InputMapper.SetShouldRun(false);
            
            _stateMachine.ResetRestTimer();
        }

        public override void Update()
        {
            if (_stateMachine.Vision.HasTarget)
            {
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }
            
            _restTimer += Time.deltaTime;
            
            if (_restTimer >= _restDuration)
            {
                _stateMachine.SwitchState(AIStateType.Patrol);
            }
        }

        public override void Exit()
        {
            _stateMachine.Navigation.Resume();
        }
    }
}