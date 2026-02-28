using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public enum PlayerState
    {
        Base,
        Locomotion,
        Jump,
        Fall,
        Crouch
    }

    public class PlayerStateMachine
    {
        private Dictionary<PlayerState, IPlayerState> _states = new Dictionary<PlayerState, IPlayerState>();
        private IPlayerState _currentState;
        private PlayerState _currentStateType;
        private PlayerHandler _handler;

        public PlayerState CurrentStateType => _currentStateType;
        public PlayerHandler Handler => _handler;

        public PlayerStateMachine(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void RegisterState(PlayerState stateType, IPlayerState state)
        {
            _states[stateType] = state;
        }

        public void SwitchState(PlayerState newStateType)
        {
            if (_states.ContainsKey(newStateType))
            {
                _currentState?.Exit();
                _currentStateType = newStateType;
                _currentState = _states[newStateType];
                _currentState.Enter();
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }

    public interface IPlayerState
    {
        void Enter();
        void Update();
        void Exit();
    }
}