using UnityEngine;
using Character.InputController;

namespace Enemy.Navigation
{
    public class AIInputReader : BaseInputReader
    { 
        public void SetMoveDirection(Vector2 moveDirection)
        {
            if(_allInputBlocked)
                return;

            MoveComposite = moveDirection;
            MovementInputDetected = MoveComposite.magnitude > 0;
        }

        public void SetLookDirection(Vector2 lookDirection)
        {
            if(_allInputBlocked)
                return;

            MouseDelta = lookDirection;

        }
        
        public void SetSprint(bool value)
        {
            if(_allInputBlocked)
                return;
            
            if (value)
                OnSprintActivated?.Invoke();
            else
                OnSprintDeactivated?.Invoke();
        }

        
        public void PerformPrimaryAttack()
        {
            if(CanProcessWeaponInput())
                return;

            OnPrimaryAttackActivated?.Invoke();
        }

        public void PerformSecondaryAttack()
        {
            if(CanProcessWeaponInput())
                return;

            OnSecondaryAttackActivated?.Invoke();
        }

    }
}