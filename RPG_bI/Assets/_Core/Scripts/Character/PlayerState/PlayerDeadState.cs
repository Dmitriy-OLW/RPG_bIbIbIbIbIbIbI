using UnityEngine;

namespace Character
{
    public class PlayerDeadState : PlayerBaseState
    {
        private bool _deathAnimationFinished;
        private float _stateTimer;
        
        public delegate void PlayerDeathHandler();
        public static event PlayerDeathHandler OnPlayerDeath;
        
        public PlayerDeadState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {

            _handler.InputReader.SetAllInputBlock(true);
            
            _handler.AnimatorController.SetIsDead(true);
            _handler.AnimatorController.SetHitRandomizer();
            _handler.AnimatorController.TriggerHitAnimation();
            
            _deathAnimationFinished = false;
            _stateTimer = 0f;
        }

        public override void Update()
        {
            if (!_deathAnimationFinished)
            {
                _stateTimer += Time.deltaTime;
                
                if (_handler.AnimatorController.GetAnimationHitState() || _stateTimer > 10.0f) 
                {
                    _deathAnimationFinished = true;
                }
            }
            if (!_handler.PlayerGroundedChecker.IsGrounded)
            {
                _handler.PlayerMovement.ApplyGravity();
            }
            
            _handler.PlayerMovement.Move();
        }

        public override void Exit()
        {
            OnPlayerDeath?.Invoke();
        }
    }
}