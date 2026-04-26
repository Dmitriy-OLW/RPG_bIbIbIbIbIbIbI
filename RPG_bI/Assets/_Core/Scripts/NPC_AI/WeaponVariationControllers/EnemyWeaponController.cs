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
        [SerializeField] protected StrategyData _primaryStrategy;
        
        [Header("Secondary Weapon")]
        [SerializeField] protected StrategyData _secondaryStrategy;
        
        [Header("Attack Settings")]
        [SerializeField] protected AttackPriority _preferredAttack = AttackPriority.Primary;
        
        [SerializeField] protected WeaponController _weaponController;
        
        public AttackPriority PreferredAttack => _preferredAttack;

        public EnemyType GetWeaponType(WeaponStateActive weaponSlot)
        {
            return _weaponController.GetWeaponType(weaponSlot);
        }
        
        public StrategyData GetStrategy(bool isPrimary)
        {
            return isPrimary ? _primaryStrategy : _secondaryStrategy;
        }
    }
}