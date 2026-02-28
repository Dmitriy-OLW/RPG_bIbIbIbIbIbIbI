using Character.InputController;
using UnityEngine;

namespace Character
{
    public class PlayerLocomotionState : PlayerBaseState
    {
        public PlayerLocomotionState(PlayerStateMachine stateMachine, PlayerHandler handler) : base(stateMachine, handler)
        {
        }

        public override void Enter()
        {
            _handler.InputReader.onJumpPerformed += LocomotionToJumpState;
        }

        public override void Update()
        {
            _handler.PlayerGroundedChecker.GroundedCheck();
            _handler.PlayerGroundedChecker.GroundInclineCheck();

            if (!_handler.PlayerGroundedChecker.IsGrounded)
            {
                _stateMachine.SwitchState(PlayerState.Fall);
                return;
            }

            if (_handler.PlayerCrouch.IsCrouching)
            {
                _stateMachine.SwitchState(PlayerState.Crouch);
                return;
            }

            _handler.PlayerLocomotionAdditives.CheckEnableTurns();
            _handler.PlayerLocomotionAdditives.CheckEnableLean();
            _handler.PlayerLocomotionAdditives.CalculateRotationalAdditives(
                _handler.PlayerLocomotionAdditives.EnableLean,
                _handler.PlayerLocomotionAdditives.EnableHeadTurn,
                _handler.PlayerLocomotionAdditives.EnableBodyTurn
            );

            _handler.PlayerMovement.CalculateMoveDirection();
            _handler.PlayerMovement.CheckIfStarting();
            _handler.PlayerMovement.CheckIfStopped();
            _handler.PlayerRotation.FaceMoveDirection();
            _handler.PlayerMovement.Move();
            _handler.PlayerMovement.UpdateAnimatorController();
        }

        public override void Exit()
        {
            _handler.InputReader.onJumpPerformed -= LocomotionToJumpState;
        }

        private void LocomotionToJumpState()
        {
            _stateMachine.SwitchState(PlayerState.Jump);
        }
    }
}