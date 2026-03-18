using System.Collections.Generic;
using Damage;
using UnityEngine;

namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IAttackable
    {
        [SerializeField] protected DamageDataSO _damageDataSO;
        
        public abstract void Attack();
    }
}