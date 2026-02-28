using Character.InputController;
using UnityEngine;

namespace Character
{
    public class PlayerInputHandler
    {
        private PlayerHandler _handler;

        public PlayerInputHandler(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void SubscribeToInputEvents()
        {
            _handler.InputReader.onLockOnToggled += OnLockOnToggled;
            _handler.InputReader.onWalkToggled += OnWalkToggled;
            _handler.InputReader.onSprintActivated += OnSprintActivated;
            _handler.InputReader.onSprintDeactivated += OnSprintDeactivated;
            _handler.InputReader.onCrouchActivated += OnCrouchActivated;
            _handler.InputReader.onCrouchDeactivated += OnCrouchDeactivated;
            _handler.InputReader.onAimActivated += OnAimActivated;
            _handler.InputReader.onAimDeactivated += OnAimDeactivated;
        }

        public void UnsubscribeFromInputEvents()
        {
            _handler.InputReader.onLockOnToggled -= OnLockOnToggled;
            _handler.InputReader.onWalkToggled -= OnWalkToggled;
            _handler.InputReader.onSprintActivated -= OnSprintActivated;
            _handler.InputReader.onSprintDeactivated -= OnSprintDeactivated;
            _handler.InputReader.onCrouchActivated -= OnCrouchActivated;
            _handler.InputReader.onCrouchDeactivated -= OnCrouchDeactivated;
            _handler.InputReader.onAimActivated -= OnAimActivated;
            _handler.InputReader.onAimDeactivated -= OnAimDeactivated;
        }

        private void OnLockOnToggled()
        {
            _handler.PlayerTargeting.ToggleLockOn();
        }

        private void OnWalkToggled()
        {
            _handler.PlayerRotation.SetIsWalking(!_handler.PlayerRotation.IsWalking);
        }

        private void OnSprintActivated()
        {
            if (!_handler.PlayerCrouch.IsCrouching)
            {
                _handler.PlayerRotation.SetIsWalking(false);
                _handler.PlayerRotation.SetIsSprinting(true);
                _handler.PlayerRotation.SetIsStrafing(false);
            }
        }

        private void OnSprintDeactivated()
        {
            _handler.PlayerRotation.SetIsSprinting(false);
            _handler.PlayerRotation.UpdateStrafingState();
        }

        private void OnCrouchActivated()
        {
            _handler.PlayerCrouch.ActivateCrouch();
        }

        private void OnCrouchDeactivated()
        {
            _handler.PlayerCrouch.DeactivateCrouch();
        }

        private void OnAimActivated()
        {
            _handler.PlayerTargeting.SetIsAiming(true);
        }

        private void OnAimDeactivated()
        {
            _handler.PlayerTargeting.SetIsAiming(false);
        }
    }
}