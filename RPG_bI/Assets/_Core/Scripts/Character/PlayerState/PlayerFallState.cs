using UnityEngine;

namespace Character
{
    public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {
            _handler.PlayerMovement.ResetFallingDuration();
            _handler.PlayerMovement.SetVelocityY(0f);
            _handler.PlayerCrouch.DeactivateCrouch();
            _handler.PlayerCrouch.DeactivateSliding();
        }

        public override void Update()
        {
            _handler.PlayerGroundedChecker.GroundedCheck();
            _handler.PlayerLocomotionAdditives.CalculateRotationalAdditives(false,
                _handler.PlayerLocomotionAdditives.EnableHeadTurn,
                _handler.PlayerLocomotionAdditives.EnableBodyTurn);
            _handler.PlayerMovement.CalculateMoveDirection();
            _handler.PlayerRotation.FaceMoveDirection();
            _handler.PlayerMovement.ApplyGravity();
            _handler.PlayerMovement.Move();
            _handler.PlayerMovement.UpdateAnimatorController();
            _handler.PlayerMovement.UpdateFallingDuration();

            if (_handler.Controller.isGrounded)
            {
                _stateMachine.SwitchState(PlayerState.Locomotion);
            }
        }

        public override void Exit()
        {
        }
    }
}