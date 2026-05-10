using UnityEngine;
using Weapons;

namespace Weapons
{
    public class WeaponControllerBoss : WeaponController
    {
        protected override void OnPrimaryAttack()
        {
            if (_currentWeaponState == WeaponStateActive.Deactive)
            {
                SetRandomAnimationBasedOnWeaponType(WeaponStateActive.PrimaryActive);
                _animator.SetTrigger(PrimaryAttack);
                _currentWeaponState = WeaponStateActive.PrimaryActive;
            }
        }

        protected override void OnSecondaryAttack()
        {
            if (_currentWeaponState == WeaponStateActive.Deactive)
            {
                SetRandomAnimationBasedOnWeaponType(WeaponStateActive.SecondaryActive);
                _animator.SetTrigger(SecondaryAttack);
                _currentWeaponState = WeaponStateActive.SecondaryActive;
            }
        }

        private void SetRandomAnimationBasedOnWeaponType(WeaponStateActive weaponSlot)
        {
            EnemyType weaponType = GetWeaponType(weaponSlot);
            
            if (weaponType == EnemyType.Melee)
            {
                _animator.SetBool(RandomAttack, false);
            }
            else if (weaponType == EnemyType.Ranged)
            {
                _animator.SetBool(RandomAttack, true);
            }
        }
    }
}