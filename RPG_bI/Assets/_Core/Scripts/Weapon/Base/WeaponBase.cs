using System.Collections.Generic;
using Damage;
using UnityEngine;

namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IAttackable
    {
        [SerializeField] protected List<DamageData> _damageDataList;
        
        public abstract void Attack();
    }
}