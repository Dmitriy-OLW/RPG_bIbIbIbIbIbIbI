using System;
using UnityEngine;
using System.Collections.Generic;

namespace Character.Targeting
{
    [CreateAssetMenu(fileName = "FactionRelations", menuName = "RPG/Faction Relations")]
    public class FactionRelationsSO : ScriptableObject
    {
        [SerializeField] private List<FactionRelationData> _factionRelations = new List<FactionRelationData>();
        
        private Dictionary<FactionType, FactionRelationData> _relationLookup;
        
        public void Initialize()
        {
            _relationLookup = new Dictionary<FactionType, FactionRelationData>();
            foreach (var data in _factionRelations)
            {
                _relationLookup[data.Faction] = data;
            }
        }
        
        public bool IsHostile(FactionType attacker, FactionType victim)
        {
            if (_relationLookup == null) Initialize();
            
            if (_relationLookup.TryGetValue(attacker, out var data))
            {
                return data.Enemies.Contains(victim);
            }
            
            return false;
        }
        
        public bool IsFriendly(FactionType a, FactionType b)
        {
            return a == b;
        }
        
        public FactionRelationData GetRelationData(FactionType faction)
        {
            if (_relationLookup == null) Initialize();
            _relationLookup.TryGetValue(faction, out var data);
            return data;
        }
    }
    
    [Serializable]
    public class FactionRelationData
    {
        public FactionType Faction;
        public List<FactionType> Enemies = new List<FactionType>();
    }
}