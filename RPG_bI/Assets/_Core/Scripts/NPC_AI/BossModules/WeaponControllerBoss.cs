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
                Debug.Log("VAR");
                SetRandomAnimationBasedOnWeaponType(WeaponStateActive.PrimaryActive);
                _animator.SetTrigger(PrimaryAttack);
                _currentWeaponState = WeaponStateActive.PrimaryActive;
            }
        }

        protected override void OnSecondaryAttack()
        {
            if (_currentWeaponState == WeaponStateActive.Deactive)
            {
                Debug.Log("324424234123421`32134");
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
                Debug.Log("11111111111111111111");
                _animator.SetBool(RandomAttack, false);
            }
            else if (weaponType == EnemyType.Ranged)
            {
                Debug.Log("22222222222222222222222222222222");
                _animator.SetBool(RandomAttack, true);
            }
        }
    }
}