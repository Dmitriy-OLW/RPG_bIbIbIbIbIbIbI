using UnityEngine;
using Weapons;
using Enemy.Strategies;

namespace Enemy.Weapons
{
    public enum AttackPriority
    {
        Primary,
        Secondary
    }
    
    public class EnemyWeaponController : MonoBehaviour, IEnemyWeaponProvider
    {
        [Header("Primary Weapon")]
        [SerializeField] protected WeaponBase _primaryWeapon;
        [SerializeField] protected StrategyData _primaryStrategy;
        
        [Header("Secondary Weapon")]
        [SerializeField] protected WeaponBase _secondaryWeapon;
        [SerializeField] protected StrategyData _secondaryStrategy;
        
        [Header("Attack Settings")]
        [SerializeField] protected AttackPriority _preferredAttack = AttackPriority.Primary;
        
        [SerializeField] protected WeaponController _weaponController;
        
        public AttackPriority PreferredAttack => _preferredAttack;

        public EnemyType GetWeaponType(bool isPrimary)
        {
            switch (isPrimary)
            {
                case true:
                    return _weaponController.GetWeaponType(WeaponStateActive.PrimaryActive);
                case false:
                    return _weaponController.GetWeaponType(WeaponStateActive.SecondaryActive);
            }
        }
        
        public StrategyData GetStrategy(bool isPrimary)
        {
            return isPrimary ? _primaryStrategy : _secondaryStrategy;
        }
        
        public WeaponBase GetWeapon(bool isPrimary)
        {
            return isPrimary ? _primaryWeapon : _secondaryWeapon;
        }
    }
}