namespace Enemy.Data
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }

    public enum AIStateType
    {
        Patrol,
        Aggression,
        Attack,
        Search,
        Dead
    }

    public enum AttackType
    {
        Primary,
        Secondary
    }
}