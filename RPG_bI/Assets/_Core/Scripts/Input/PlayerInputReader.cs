using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.InputController
{
    public class PlayerInputReader : BaseInputReader
    {
        public void OnLook(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            MouseDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            MoveComposite = context.ReadValue<Vector2>();
            MovementInputDetected = MoveComposite.magnitude > 0;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed || _allInputBlocked)
                return;
            
            OnJumpPerformed?.Invoke();
        }
        
        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed || _allInputBlocked)
                return;

            OnWalkToggled?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            if (context.started)
                OnSprintActivated?.Invoke();
            else if (context.canceled)
                OnSprintDeactivated?.Invoke();
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            if (context.started)
                OnCrouchActivated?.Invoke();
            else if (context.canceled)
                OnCrouchDeactivated?.Invoke();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.started)
                OnAimActivated?.Invoke();
            if (context.canceled)
                OnAimDeactivated?.Invoke();
        }
        
        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed) 
                return;
            
            OnLockOnToggled?.Invoke();
            OnSprintDeactivated?.Invoke();
        }
        
        public void OnPrimaryAttack(InputAction.CallbackContext context)
        {
            if (!context.performed || CanProcessWeaponInput())
                return;
               
            OnPrimaryAttackActivated?.Invoke();
        }
        
        public void OnSecondaryAttack(InputAction.CallbackContext context)
        {
            if (!context.performed || CanProcessWeaponInput())
                return;
            
            OnSecondaryAttackActivated?.Invoke();
        }
    }
}