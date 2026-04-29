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
        public GameObject WeaponVisualObject;
        public bool CanBePrimary = true;
        public bool CanBeSecondary = true;
    }
    
    public class BossWeaponController : EnemyWeaponController
    {
        [Header("Weapon Configurations")]
        [SerializeField] private List<WeaponConfig> _weaponConfigs = new List<WeaponConfig>();
        
        [Header("Boss Settings")]
        [SerializeField] private bool _initializeFromConfigList = false;
        
        [SerializeField] private GameObject _currentPrimaryVisual;
        [SerializeField] private GameObject _currentSecondaryVisual;
        
        private void Start()
        {
            //DisableAllVisualObjects();
            SetupWeapons();
        }
        
        private void DisableAllVisualObjects()
        {
            foreach (var config in _weaponConfigs)
            {
                if (config.WeaponVisualObject != null)
                    config.WeaponVisualObject.SetActive(false);
            }
        }
        
        protected void SetupWeapons()
        {
            if (!_initializeFromConfigList)
                return;
            
            if (_weaponConfigs.Count >= 2)
            {
                WeaponConfig validPrimary = GetFirstValidConfig(true);
                WeaponConfig validSecondary = GetFirstValidConfig(false);
                
                if (validPrimary != null && validSecondary != null)
                {
                    SetPrimaryWeapon(validPrimary.Weapon, validPrimary.StrategyData, validPrimary.WeaponVisualObject);
                    SetSecondaryWeapon(validSecondary.Weapon, validSecondary.StrategyData, validSecondary.WeaponVisualObject);
                }
            }
        }
        
        private WeaponConfig GetFirstValidConfig(bool isPrimary)
        {
            foreach (var config in _weaponConfigs)
            {
                if (isPrimary && config.CanBePrimary)
                    return config;
                if (!isPrimary && config.CanBeSecondary)
                    return config;
            }
            return null;
        }
        
        public void SetPrimaryWeapon(WeaponBase weapon, StrategyData strategyData = null, GameObject visualObject = null)
        {
            if (strategyData != null)
                _primaryStrategy = strategyData;
            
            if (visualObject != null)
            {
                if (_currentPrimaryVisual != null)
                    _currentPrimaryVisual.SetActive(false);
                
                // DisableAllVisualObjects();
                _currentPrimaryVisual = visualObject;
                _currentPrimaryVisual.SetActive(true);
            }
            
            _weaponController?.SetWeapon(WeaponStateActive.PrimaryActive, weapon);
        }
        
        public void SetSecondaryWeapon(WeaponBase weapon, StrategyData strategyData = null, GameObject visualObject = null)
        {
            if (strategyData != null)
                _secondaryStrategy = strategyData;
            
            if (visualObject != null)
            {
                if (_currentSecondaryVisual != null)
                    _currentSecondaryVisual.SetActive(false);
                    
                _currentSecondaryVisual = visualObject;
                _currentSecondaryVisual.SetActive(false);
            }
            
            _weaponController?.SetWeapon(WeaponStateActive.SecondaryActive, weapon);
        }
        
        public void SetWeaponFromConfig(int configIndex, bool isPrimary)
        {
            if (configIndex < 0 || configIndex >= _weaponConfigs.Count) 
                return;
            
            var config = _weaponConfigs[configIndex];
            
            if (isPrimary && !config.CanBePrimary)
                return;
            
            if (!isPrimary && !config.CanBeSecondary)
                return;
            
            if (isPrimary)
            {
                SetPrimaryWeapon(config.Weapon, config.StrategyData, config.WeaponVisualObject);
            }
            else
            {
                SetSecondaryWeapon(config.Weapon, config.StrategyData, config.WeaponVisualObject);
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
        
        public List<int> GetValidConfigIndices(bool isPrimary)
        {
            List<int> validIndices = new List<int>();
            for (int i = 0; i < _weaponConfigs.Count; i++)
            {
                if (isPrimary && _weaponConfigs[i].CanBePrimary)
                    validIndices.Add(i);
                else if (!isPrimary && _weaponConfigs[i].CanBeSecondary)
                    validIndices.Add(i);
            }
            return validIndices;
        }
    }
}