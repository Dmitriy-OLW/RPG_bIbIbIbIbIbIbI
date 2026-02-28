using UnityEngine;

namespace Character
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {
            _handler.AnimatorController.SetIsJumping(true);
            _handler.PlayerCrouch.DeactivateSliding();
            _handler.PlayerMovement.SetVelocityY(_handler.Config.JumpForce);
        }

        public override void Update()
        {
            _handler.PlayerMovement.ApplyGravity();

            if (_handler.PlayerMovement.Velocity.y <= 0f)
            {
                _handler.AnimatorController.SetIsJumping(false);
                _stateMachine.SwitchState(PlayerState.Fall);
                return;
            }

            _handler.PlayerGroundedChecker.GroundedCheck();
            _handler.PlayerLocomotionAdditives.CalculateRotationalAdditives(false,
                _handler.PlayerLocomotionAdditives.EnableHeadTurn,
                _handler.PlayerLocomotionAdditives.EnableBodyTurn);
            _handler.PlayerMovement.CalculateMoveDirection();
            _handler.PlayerRotation.FaceMoveDirection();
            _handler.PlayerMovement.Move();
            _handler.PlayerMovement.UpdateAnimatorController();
        }

        public override void Exit()
        {
            _handler.AnimatorController.SetIsJumping(false);
        }
    }
}