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
        
        public Action onAimActivated;
        public Action onAimDeactivated;

        public Action onCrouchActivated;
        public Action onCrouchDeactivated;

        public Action onJumpPerformed;

        public Action onLockOnToggled;

        public Action onSprintActivated;
        public Action onSprintDeactivated;

        public Action onWalkToggled;
        
        public Vector2 MouseDelta => _mouseDelta;         
        public Vector2 MoveComposite => _moveComposite;     
                                           
        public float MovementInputDuration =>_movementInputDuration;
        public bool MovementInputDetected => _movementInputDetected; 

        public void OnLook(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveComposite = context.ReadValue<Vector2>();
            _movementInputDetected = _moveComposite.magnitude > 0;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            onJumpPerformed?.Invoke();
        }
        
        public void OnToggleWalk(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            onWalkToggled?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
                onSprintActivated?.Invoke();
            else if (context.canceled)
                onSprintDeactivated?.Invoke();
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
                onCrouchActivated?.Invoke();
            else if (context.canceled)
                onCrouchDeactivated?.Invoke();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.started)
                onAimActivated?.Invoke();

            if (context.canceled)
                onAimDeactivated?.Invoke();
        }
        
        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed) 
                return;
            
            onLockOnToggled?.Invoke();
            onSprintDeactivated?.Invoke();
        }
        
        public void OnMeleeAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {

            }
        }
        
        public void OnRangeAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {

            }
        }
    }
}
