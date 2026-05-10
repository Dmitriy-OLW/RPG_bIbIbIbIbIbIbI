namespace Weapons
{
    public interface IAttackable
    {
        EnemyType EnemyType { get; }
        void Attack();
    }
}