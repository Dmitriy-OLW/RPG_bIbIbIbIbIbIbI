using System;
using System.Collections.Generic;
using UnityEngine;
using Weapons;
using Enemy.Strategies;

namespace Enemy.Weapons
{
    [Serializable]
    public class WeaponConfig
    {
        public WeaponBase Weapon;
        public StrategyData StrategyData;
    }
    
    public class BossWeaponController : EnemyWeaponController
    {
        [Header("Weapon Configurations")]
        [SerializeField] private List<WeaponConfig> _weaponConfigs = new List<WeaponConfig>();
        
        [Header("Boss Settings")]
        [SerializeField] private bool _initializeFromConfigList = false;
        
        protected void SetupWeapons()
        {
            if (_initializeFromConfigList && _weaponConfigs.Count >= 2)
            {
                SetPrimaryWeapon(_weaponConfigs[0].Weapon, _weaponConfigs[0].StrategyData);
                SetSecondaryWeapon(_weaponConfigs[1].Weapon, _weaponConfigs[1].StrategyData);
            }
        }
        
        public void SetPrimaryWeapon(WeaponBase weapon, StrategyData strategyData = null)
        {
            if (strategyData != null)
                _primaryStrategy = strategyData;
            
            
            _weaponController?.SetWeapon(WeaponStateActive.PrimaryActive, weapon);
        }
        
        public void SetSecondaryWeapon(WeaponBase weapon, StrategyData strategyData = null)
        {
            if (strategyData != null)
                _secondaryStrategy = strategyData;
            
            
            _weaponController?.SetWeapon(WeaponStateActive.SecondaryActive, weapon);
        }
        
        
        public void SetWeaponFromConfig(int configIndex, bool isPrimary)
        {
            if (configIndex < 0 || configIndex >= _weaponConfigs.Count) 
                return;
            
            var config = _weaponConfigs[configIndex];
            
            if (isPrimary)
            {
                SetPrimaryWeapon(config.Weapon, config.StrategyData);
            }
            else
            {
                SetSecondaryWeapon(config.Weapon, config.StrategyData);
            }
        }
        
        public int GetConfigCount()
        {
            return _weaponConfigs.Count;
        }
        
        public WeaponConfig GetConfig(int index)
        {
            if (index >= 0 && index < _weaponConfigs.Count)
                return _weaponConfigs[index];

                return null;
        }
    }
}