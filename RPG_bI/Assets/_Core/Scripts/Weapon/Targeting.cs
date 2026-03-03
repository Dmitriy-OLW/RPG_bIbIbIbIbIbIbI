using UnityEngine;

namespace Character.Targeting
{
    public enum FactionType
    {
        Friendly,
        Enemy,
        Neutral
    }

    public class Targeting : MonoBehaviour
    {
        [SerializeField] private FactionType _faction = FactionType.Neutral;
        
        public FactionType Faction => _faction;
        
        public bool IsHostileTowards(Targeting other)
        {
            if (other == null || _faction == FactionType.Neutral || other._faction == FactionType.Neutral)
                return false;
                
            return _faction != other._faction;
        }
        
        public bool IsSameFaction(Targeting other)
        {
            if (other == null) return false;
            return _faction == other._faction;
        }
    }
}