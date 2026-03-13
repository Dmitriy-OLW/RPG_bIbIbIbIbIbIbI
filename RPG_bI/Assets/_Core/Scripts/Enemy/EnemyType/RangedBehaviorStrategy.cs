using UnityEngine;
using Enemy.Navigation;

namespace Enemy.Strategies
{
    public class RangedBehaviorStrategy : IEnemyBehaviorStrategy
    {
        public float AggressionRange => 15f;
        public float AttackRange => 12f;
        public float PreferredDistance => 8f;
        public float AttackCooldown => 5f;
        
        public void UpdatePosition(Transform enemy, Transform target, out bool shouldAttack)
        {
            shouldAttack = false;
            
            if (target == null)
                return;
                
            float distanceToTarget = Vector3.Distance(enemy.position, target.position);
            
            if (distanceToTarget <= AttackRange && distanceToTarget >= 5f)
            {
                shouldAttack = true;
            }
        }
        
        public void PerformAttack(AIInputReader inputReader)
        {
            inputReader.PerformSecondaryAttack();
        }
    }
}