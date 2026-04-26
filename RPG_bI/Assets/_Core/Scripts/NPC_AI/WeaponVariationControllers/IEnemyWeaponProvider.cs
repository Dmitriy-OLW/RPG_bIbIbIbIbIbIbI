using UnityEngine;
using Weapons;
using Enemy.Strategies;

namespace Enemy.Weapons
{
    public interface IEnemyWeaponProvider
    {
        EnemyType GetWeaponType(WeaponStateActive weaponSlot);
        StrategyData GetStrategy(bool isPrimary);
        AttackPriority PreferredAttack { get; }
    }
}