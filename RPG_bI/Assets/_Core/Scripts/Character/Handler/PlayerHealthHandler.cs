using Health;
using UnityEngine;

namespace Character
{
    public class PlayerHealthHandler
    {
        private readonly HealthController _healthController;
        private readonly PlayerStateMachine _stateMachine;

        public PlayerHealthHandler(HealthController healthController, PlayerStateMachine stateMachine)
        {
            _healthController = healthController;
            _stateMachine = stateMachine;
        }

        public void SubscribeToHealthEvents()
        {
            _healthController.OnHit += HandleHit;
            _healthController.OnDeath += HandleDeath;
        }

        public void UnsubscribeFromHealthEvents()
        {
            _healthController.OnHit -= HandleHit;
            _healthController.OnDeath -= HandleDeath;
        }

        private void HandleHit()
        {
            if (_stateMachine.CurrentStateType == PlayerState.Dead || 
                _stateMachine.CurrentStateType == PlayerState.Hit)
                return;
            
            _stateMachine.SwitchState(PlayerState.Hit);
        }

        private void HandleDeath()
        {
            _stateMachine.SwitchState(PlayerState.Dead);
        }
    }
}