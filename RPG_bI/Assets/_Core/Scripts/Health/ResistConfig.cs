using UnityEngine;
using System.Collections.Generic;
using Damage;

namespace Health
{
    [CreateAssetMenu(fileName = "NewResistConfig", menuName = "RPG/Resist Config")]
    public class ResistConfig : ScriptableObject
    {
        [SerializeField] private List<DamageResistData> _resists;
        
        public float GetResistMultiplier(DamageType damageType)
        {
            foreach (var resist in _resists)
            {
                if (resist.Type == damageType)
                {
                    return resist.ResistMultiplier;
                }
            }
            
            return 1f;
        }
    }
}