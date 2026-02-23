using System.Collections.Generic;
using Damage;

namespace Health
{
    public interface IDamageable
    {
        void Damage(List<DamageData> damages);
        float CurrentHealth { get; }
    }
}