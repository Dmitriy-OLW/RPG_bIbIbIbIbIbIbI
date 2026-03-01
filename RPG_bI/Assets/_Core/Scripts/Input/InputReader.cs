using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Character.InputController
{
    public class InputReader : MonoBehaviour
    {
        public Vector2 _mouseDelta;
        public Vector2 _moveComposite;

        public float _movementInputDuration;
        public bool _movementInputDetected;
        
        [SerializeField] private bool _weaponInputBlocked;
        [SerializeField] private bool _allInputBlocked;
        
        public Action onAimActivated;
        public Action onAimDeactivated;

        public Action onCrouchActivated;
        public Action onCrouchDeactivated;

        public Action onJumpPerformed;

        public Action onLockOnToggled;

        public Action onSprintActivated;
        public Action onSprintDeactivated;

        public Action onWalkToggled;
        
        public Action onPrimaryAttack;
        public Action onSecondaryAttack;
        
        public Vector2 MouseDelta => _mouseDelta;

        public void SetAllInputBlock(bool value)
        {
            _allInputBlocked = value;
        }
        
        public void SetWeaponBlock(bool value)
        {
            _weaponInputBlocked = value;
        }
        
        public void OnLook(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            _mouseDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            _moveComposite = context.ReadValue<Vector2>();
            _movementInputDetected = _moveComposite.magnitude > 0;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed || _allInputBlocked)
                return;
            
            onJumpPerformed?.Invoke();
        }
        
        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed || _allInputBlocked)
                return;

            onWalkToggled?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            if (context.started)
                onSprintActivated?.Invoke();
            else if (context.canceled)
                onSprintDeactivated?.Invoke();
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            if (context.started)
                onCrouchActivated?.Invoke();
            else if (context.canceled)
                onCrouchDeactivated?.Invoke();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if(_allInputBlocked)
                return;
            
            if (context.started)
                onAimActivated?.Invoke();

            if (context.canceled)
                onAimDeactivated?.Invoke();
        }
        
        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed || _allInputBlocked) 
                return;
            
            onLockOnToggled?.Invoke();
            onSprintDeactivated?.Invoke();
        }
        
        public void OnPrimaryAttack(InputAction.CallbackContext context)
        {
            if (!context.performed || CanProcessWeaponInput())
                return;
               
            onPrimaryAttack?.Invoke();
            
        }
        
        public void OnSecondaryAttack(InputAction.CallbackContext context)
        {
            if (!context.performed || CanProcessWeaponInput())
                return;
            
            onSecondaryAttack?.Invoke();
            
        }

        private bool CanProcessWeaponInput()
        {
            return _allInputBlocked || _weaponInputBlocked;
        }
    }
}
