using Damage;
using System;
using UnityEngine;

namespace Health
{
    [Serializable]
    public class DamageResistData
    {
        public DamageType Type;
        
        [Range(0f, 1f)]
        public float ResistMultiplier = 1f;
    }
}