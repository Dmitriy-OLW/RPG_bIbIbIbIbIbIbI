using UnityEngine;

namespace Enemy.Strategies
{
    [CreateAssetMenu(fileName = "StrategyData", menuName = "AI/Strategy Data")]
    public class StrategyData : ScriptableObject
    {
        [SerializeField] private float _aggressionRange = 10f;
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _preferredDistance = 1.5f;
        [SerializeField] private float _attackCooldown = 4f;
        
        public float AggressionRange => _aggressionRange;
        public float AttackRange => _attackRange;
        public float PreferredDistance => _preferredDistance;
        public float AttackCooldown => _attackCooldown;
    }
}