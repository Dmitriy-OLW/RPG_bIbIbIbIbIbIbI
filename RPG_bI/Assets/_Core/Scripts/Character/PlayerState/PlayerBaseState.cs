using UnityEngine;

namespace Character
{
    public class PlayerBaseState : IPlayerState
    {
        protected PlayerStateMachine _stateMachine;
        protected PlayerHandler _handler;

        public PlayerBaseState(PlayerStateMachine stateMachine, PlayerHandler handler)
        {
            _stateMachine = stateMachine;
            _handler = handler;
        }

        public virtual void Enter()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {
        }
    }
}