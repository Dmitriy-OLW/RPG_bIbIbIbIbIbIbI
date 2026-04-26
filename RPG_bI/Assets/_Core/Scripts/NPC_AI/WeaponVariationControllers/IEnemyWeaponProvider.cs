using Weapons;
using Enemy.Strategies;

namespace Enemy.Weapons
{
    public interface IEnemyWeaponProvider
    {
        EnemyType GetWeaponType(bool isPrimary);
        StrategyData GetStrategy(bool isPrimary);
        AttackPriority PreferredAttack { get; }
    }
}