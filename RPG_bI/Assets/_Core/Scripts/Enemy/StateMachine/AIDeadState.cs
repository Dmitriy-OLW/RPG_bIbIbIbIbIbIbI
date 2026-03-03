using UnityEngine;

namespace Enemy.State
{
    public class AIDeadState : IAIState
    {
        private AIStateMachine _stateMachine;
        private float _stateTimer;

        public AIDeadState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _stateMachine.Navigation.Stop();
            _stateMachine.Navigation.enabled = false;
            //_stateMachine.InputMapper.ResetInput();
            _stateMachine.InputMapper.enabled = false;
            _stateMachine.Vision.enabled = false;
            
            _stateTimer = 0f;
        }

        public void Update()
        {
            _stateTimer += Time.deltaTime;
            
            if (_stateTimer > 5f)
            {
                GameObject.Destroy(_stateMachine.gameObject);
            }
        }

        public void Exit()
        {
        }
    }
}