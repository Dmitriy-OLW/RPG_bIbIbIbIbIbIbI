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
            _handler.InputReader.OnLockOnToggled += OnLockOnToggled;
            _handler.InputReader.OnWalkToggled += OnWalkToggled;
            _handler.InputReader.OnSprintActivated += OnSprintActivated;
            _handler.InputReader.OnSprintDeactivated += OnSprintDeactivated;
            _handler.InputReader.OnCrouchActivated += OnCrouchActivated;
            _handler.InputReader.OnCrouchDeactivated += OnCrouchDeactivated;
            _handler.InputReader.OnAimActivated += OnAimActivated;
            _handler.InputReader.OnAimDeactivated += OnAimDeactivated;
        }

        public void UnsubscribeFromInputEvents()
        {
            _handler.InputReader.OnLockOnToggled -= OnLockOnToggled;
            _handler.InputReader.OnWalkToggled -= OnWalkToggled;
            _handler.InputReader.OnSprintActivated -= OnSprintActivated;
            _handler.InputReader.OnSprintDeactivated -= OnSprintDeactivated;
            _handler.InputReader.OnCrouchActivated -= OnCrouchActivated;
            _handler.InputReader.OnCrouchDeactivated -= OnCrouchDeactivated;
            _handler.InputReader.OnAimActivated -= OnAimActivated;
            _handler.InputReader.OnAimDeactivated -= OnAimDeactivated;
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