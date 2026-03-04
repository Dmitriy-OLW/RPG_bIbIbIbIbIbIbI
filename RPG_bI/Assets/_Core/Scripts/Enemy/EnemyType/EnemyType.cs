using UnityEngine;
using Enemy.Navigation;

namespace Enemy.Strategies
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }
    

    public interface IEnemyBehaviorStrategy
    {
        float AggressionRange { get; }      
        float AttackRange { get; }           
        float PreferredDistance { get; }      
        float MoveSpeed { get; }              
        float AttackCooldown { get; }        
        
        void UpdatePosition(Transform enemy, Transform target, out bool shouldAttack);
        void PerformAttack(AIInputReader inputReader);
    }
}