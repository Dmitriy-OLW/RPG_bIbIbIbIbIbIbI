using Character.InputController;
using UnityEngine;

namespace Character
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {
            _handler.InputReader.onJumpPerformed += CrouchToJumpState;
        }

        public override void Update()
        {
            _handler.PlayerGroundedChecker.GroundedCheck();
            
            if (!_handler.PlayerGroundedChecker.IsGrounded)
            {
                _handler.PlayerCrouch.DeactivateCrouch();
                _handler.PlayerCrouch.CapsuleCrouchingSize(false);
                _stateMachine.SwitchState(PlayerState.Fall);
                return;
            }

            _handler.PlayerCrouch.CeilingHeightCheck();

            if (!_handler.PlayerCrouch.CrouchKeyPressed && !_handler.PlayerCrouch.CannotStandUp)
            {
                _handler.PlayerCrouch.DeactivateCrouch();
                _stateMachine.SwitchState(PlayerState.Locomotion);
                return;
            }

            if (!_handler.PlayerCrouch.IsCrouching)
            {
                _handler.PlayerCrouch.CapsuleCrouchingSize(false);
                _stateMachine.SwitchState(PlayerState.Locomotion);
                return;
            }

            _handler.PlayerLocomotionAdditives.CheckEnableTurns();
            _handler.PlayerLocomotionAdditives.CheckEnableLean();
            _handler.PlayerLocomotionAdditives.CalculateRotationalAdditives(false,
                _handler.PlayerLocomotionAdditives.EnableHeadTurn,
                false);

            _handler.PlayerMovement.CalculateMoveDirection();
            _handler.PlayerMovement.CheckIfStarting();
            _handler.PlayerMovement.CheckIfStopped();
            _handler.PlayerRotation.FaceMoveDirection();
            _handler.PlayerMovement.Move();
            _handler.PlayerMovement.UpdateAnimatorController();
        }

        public override void Exit()
        {
            _handler.InputReader.onJumpPerformed -= CrouchToJumpState;
        }

        private void CrouchToJumpState()
        {
            if (!_handler.PlayerCrouch.CannotStandUp)
            {
                _handler.PlayerCrouch.DeactivateCrouch();
                _stateMachine.SwitchState(PlayerState.Jump);
            }
        }
    }
}