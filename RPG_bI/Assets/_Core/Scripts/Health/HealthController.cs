using System.Collections.Generic;
using UnityEngine;
using Damage;
using Unity.Collections;

namespace Health
{
    public class HealthController : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _startHealth = 100f;
        
        [SerializeField] [ReadOnly] private float _currentHealth;
        
        [Header("Resist Settings")]
        [SerializeField] private ResistConfig _resistConfig;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float HealthPercentage => _currentHealth / _maxHealth;

        private void Awake()
        {
            _currentHealth = Mathf.Clamp(_startHealth, 0f, _maxHealth);
        }

        #region Heal

        public void Heal(float healAmount)
        {
            if (healAmount <= 0f) return;
            
            _currentHealth = Mathf.Min(_currentHealth + healAmount, _maxHealth);
        }

        public void HealPercent(float percent)
        {
            if (percent <= 0f || percent > 1f) return;
            
            float healAmount = _maxHealth * percent;
            Heal(healAmount);
        }

        #endregion

        #region Damage

        public void Damage(List<DamageData> damages)
        {
            if (damages == null || damages.Count == 0 || _currentHealth <= 0f)
                return;

            float totalDamage = CalculateTotalDamage(damages);
            
            _currentHealth = Mathf.Max(_currentHealth - totalDamage, 0f);
        }

        private float CalculateTotalDamage(List<DamageData> damages)
        {
            float totalDamage = 0f;

            foreach (var damageData in damages)
            {
                float resistMultiplier = 1;
                
                if(_resistConfig != null)
                    resistMultiplier = _resistConfig.GetResistMultiplier(damageData.Type);
                
                totalDamage += damageData.Amount * resistMultiplier;
            }

            return totalDamage;
        }

        #endregion
    }
}