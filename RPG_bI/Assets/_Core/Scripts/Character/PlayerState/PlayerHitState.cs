using Character.InputController;
using UnityEngine;

namespace Character
{
    public class PlayerHitState : PlayerBaseState
    {
        private bool _animationFinished;
        private float _stateTimer;
        
        public PlayerHitState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {

            _handler.InputReader.SetAllInputBlock(true);
            
            _handler.AnimatorController.SetHitRandomizer();
            
            _handler.AnimatorController.TriggerHitAnimation();
            
            _animationFinished = false;
            _stateTimer = 0f;

            _handler.PlayerCrouch.DeactivateCrouch();
            _handler.PlayerCrouch.DeactivateSliding();
        }

        public override void Update()
        {
            if (!_animationFinished)
            {
                _stateTimer += Time.deltaTime;
                
                if (_handler.AnimatorController.GetAnimationHitState() || _stateTimer > 10.0f) 
                {
                    _animationFinished = true;
                }
            }
            
            if (_animationFinished)
            {
                if (_handler.PlayerGroundedChecker.IsGrounded)
                {
                    _stateMachine.SwitchState(PlayerState.Locomotion);
                }
                else
                {
                    _stateMachine.SwitchState(PlayerState.Fall);
                }
                return;
            }
            
            _handler.PlayerGroundedChecker.GroundedCheck();
            
            if (!_handler.PlayerGroundedChecker.IsGrounded)
            {
                _handler.PlayerMovement.ApplyGravity();
            }
            
            _handler.PlayerMovement.Move();
        }

        public override void Exit()
        {
            _handler.InputReader.SetAllInputBlock(false);
        }
    }
}