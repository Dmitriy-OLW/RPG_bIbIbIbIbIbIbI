using UnityEngine;
using System;

namespace Character.InputController
{
    public abstract class BaseInputReader : MonoBehaviour
    {
        public Vector2 MouseDelta { get; protected set; }
        public Vector2 MoveComposite { get; protected set; }

        public bool MovementInputDetected { get; protected set; }
        
        protected bool _weaponInputBlocked;
        protected bool _allInputBlocked;
        
        public Action OnAimActivated;
        public Action OnAimDeactivated;
        public Action OnCrouchActivated;
        public Action OnCrouchDeactivated;
        public Action OnJumpPerformed;
        public Action OnLockOnToggled;
        public Action OnSprintActivated;
        public Action OnSprintDeactivated;
        public Action OnWalkToggled;
        public Action OnPrimaryAttackActivated;
        public Action OnSecondaryAttackActivated;

        public virtual void SetAllInputBlock(bool value) => _allInputBlocked = value;
        public virtual void SetWeaponBlock(bool value) => _weaponInputBlocked = value;
        
        protected virtual bool CanProcessWeaponInput() => _allInputBlocked || _weaponInputBlocked;
    }
}