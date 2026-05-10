using UnityEngine;
using Enemy.Navigation;
using Weapons;

namespace Enemy.Strategies
{
    public class MeleeBehaviorStrategy : IEnemyBehaviorStrategy
    {
        public float AggressionRange => 10f;
        public float AttackRange => 3f;
        public float PreferredDistance => 1.5f;
        public float AttackCooldown => 4f;
        
        public void UpdatePosition(Transform enemy, Transform target, out bool shouldAttack)
        {
            shouldAttack = false;
            
            if (target == null)
                return;
                
            float distanceToTarget = Vector3.Distance(enemy.position, target.position);
            
            if (distanceToTarget <= AttackRange)
            {
                shouldAttack = true;
            }
        }
        
        public void PerformAttack(AIInputReader inputReader)
        {
            inputReader.PerformPrimaryAttack();
        }
    }
}