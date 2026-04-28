using System.Collections.Generic;
using UnityEngine;
using Enemy.Weapons;
using Health;
using Weapons;

namespace Enemy.Boss
{
    public class BossWeaponSwitcher : MonoBehaviour
    {
        [SerializeField] private BossWeaponController _weaponController;
        [SerializeField] private HealthController _healthController;
        
        [SerializeField] private bool _enableOnHitSwitch = true;
        [SerializeField] private bool _enableOnHealthThresholdSwitch = true;
        [SerializeField] private bool _enableTimerSwitch = true;
        
        [SerializeField] private float _hitSwitchChance = 0.3f;
        
        [SerializeField] private float _healthThreshold = 0.5f;
        [SerializeField] private bool _switchOnlyOnceAtThreshold = true;
        [SerializeField] private List<float> _additionalThresholds = new List<float>();
        
        [SerializeField] private float _minSwitchInterval = 8f;
        [SerializeField] private float _maxSwitchInterval = 15f;
        
        [SerializeField] private bool _swapPrimaryAndSecondary = true;
        [SerializeField] private bool _useRandomConfigForPrimary = true;
        [SerializeField] private bool _useRandomConfigForSecondary = true;
        
        [SerializeField] private float _globalSwitchCooldown = 2f;
        
        private float _currentSwitchTimer;
        private float _nextTimerSwitch;
        private bool _hasTriggeredThresholdSwitch;
        private List<float> _triggeredThresholds = new List<float>();
        private bool _isSwitching;
        
        private void Awake()
        {
            if (_weaponController == null)
                _weaponController = GetComponent<BossWeaponController>();
                
            if (_healthController == null)
                _healthController = GetComponent<HealthController>();
        }
        
        private void OnEnable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit += OnHitReceived;
            }
        }
        
        private void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit -= OnHitReceived;
            }
        }
        
        private void Start()
        {
            ResetTimerSwitch();
        }
        
        private void Update()
        {
            if (_globalSwitchCooldown > 0)
            {
                _currentSwitchTimer -= Time.deltaTime;
            }
            
            if (_enableTimerSwitch)
            {
                _nextTimerSwitch -= Time.deltaTime;
                
                if (_nextTimerSwitch <= 0f && CanSwitch())
                {
                    PerformTimerSwitch();
                }
            }
            
            if (_enableOnHealthThresholdSwitch)
            {
                CheckHealthThreshold();
            }
        }
        
        private bool CanSwitch()
        {
            return !_isSwitching && _currentSwitchTimer <= 0f && _weaponController.GetConfigCount() >= 2;
        }
        
        private void OnHitReceived()
        {
            if (!_enableOnHitSwitch) return;
            if (!CanSwitch()) return;
            
            if (Random.value < _hitSwitchChance)
            {
                PerformRandomSwitch();
            }
        }
        
        private void CheckHealthThreshold()
        {
            if (_healthController == null) return;
            if (!CanSwitch()) return;
            
            float currentPercentage = _healthController.HealthPercentage;
            
            if (!_hasTriggeredThresholdSwitch && currentPercentage <= _healthThreshold)
            {
                if (_switchOnlyOnceAtThreshold)
                {
                    _hasTriggeredThresholdSwitch = true;
                }
                PerformRandomSwitch();
                return;
            }
            
            foreach (float threshold in _additionalThresholds)
            {
                if (!_triggeredThresholds.Contains(threshold) && currentPercentage <= threshold)
                {
                    _triggeredThresholds.Add(threshold);
                    PerformRandomSwitch();
                    return;
                }
            }
        }
        
        private void PerformTimerSwitch()
        {
            if (!_enableTimerSwitch) return;
            
            PerformRandomSwitch();
            ResetTimerSwitch();
        }
        
        private void PerformRandomSwitch()
        {
            if (_weaponController.GetConfigCount() < 2) return;
            
            _isSwitching = true;
            
            int primaryConfigIndex = -1;
            int secondaryConfigIndex = -1;
            
            if (_swapPrimaryAndSecondary)
            {
                int currentPrimaryIndex = GetCurrentPrimaryConfigIndex();
                int currentSecondaryIndex = GetCurrentSecondaryConfigIndex();
                
                if (currentPrimaryIndex >= 0 && currentSecondaryIndex >= 0)
                {
                    primaryConfigIndex = currentSecondaryIndex;
                    secondaryConfigIndex = currentPrimaryIndex;
                }
            }
            
            if (primaryConfigIndex == -1 && _useRandomConfigForPrimary)
            {
                primaryConfigIndex = Random.Range(0, _weaponController.GetConfigCount());
            }
            
            if (secondaryConfigIndex == -1 && _useRandomConfigForSecondary)
            {
                do
                {
                    secondaryConfigIndex = Random.Range(0, _weaponController.GetConfigCount());
                } 
                while (secondaryConfigIndex == primaryConfigIndex);
            }
            
            if (primaryConfigIndex >= 0)
            {
                _weaponController.SetWeaponFromConfig(primaryConfigIndex, true);
            }
            
            if (secondaryConfigIndex >= 0 && secondaryConfigIndex != primaryConfigIndex)
            {
                _weaponController.SetWeaponFromConfig(secondaryConfigIndex, false);
            }
            
            _currentSwitchTimer = _globalSwitchCooldown;
            _isSwitching = false;
        }
        
        public void SwitchToSpecificConfigs(int primaryIndex, int secondaryIndex)
        {
            if (!CanSwitch()) return;
            
            _isSwitching = true;
            
            if (primaryIndex >= 0 && primaryIndex < _weaponController.GetConfigCount())
            {
                _weaponController.SetWeaponFromConfig(primaryIndex, true);
            }
            
            if (secondaryIndex >= 0 && secondaryIndex < _weaponController.GetConfigCount() && secondaryIndex != primaryIndex)
            {
                _weaponController.SetWeaponFromConfig(secondaryIndex, false);
            }
            
            _currentSwitchTimer = _globalSwitchCooldown;
            _isSwitching = false;
        }
        
        public void CycleToNextWeapons()
        {
            int configCount = _weaponController.GetConfigCount();
            if (configCount < 2) return;
            
            int currentPrimaryIndex = GetCurrentPrimaryConfigIndex();
            int currentSecondaryIndex = GetCurrentSecondaryConfigIndex();
            
            int newPrimaryIndex = (currentPrimaryIndex + 1) % configCount;
            int newSecondaryIndex = (currentSecondaryIndex + 1) % configCount;
            
            if (newSecondaryIndex == newPrimaryIndex)
            {
                newSecondaryIndex = (newSecondaryIndex + 1) % configCount;
            }
            
            SwitchToSpecificConfigs(newPrimaryIndex, newSecondaryIndex);
        }
        
        private int GetCurrentPrimaryConfigIndex()
        {
            WeaponBase currentWeapon = GetCurrentPrimaryWeapon();
            if (currentWeapon == null) return -1;
            
            for (int i = 0; i < _weaponController.GetConfigCount(); i++)
            {
                WeaponConfig config = _weaponController.GetConfig(i);
                if (config != null && config.Weapon == currentWeapon)
                {
                    return i;
                }
            }
            return -1;
        }
        
        private int GetCurrentSecondaryConfigIndex()
        {
            WeaponBase currentWeapon = GetCurrentSecondaryWeapon();
            if (currentWeapon == null) return -1;
            
            for (int i = 0; i < _weaponController.GetConfigCount(); i++)
            {
                WeaponConfig config = _weaponController.GetConfig(i);
                if (config != null && config.Weapon == currentWeapon)
                {
                    return i;
                }
            }
            return -1;
        }
        
        private WeaponBase GetCurrentPrimaryWeapon()
        {
            if (_weaponController == null) return null;
            
            System.Reflection.FieldInfo field = typeof(BossWeaponController).GetField("_weaponController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                object weaponController = field.GetValue(_weaponController);
                if (weaponController != null)
                {
                    System.Reflection.MethodInfo method = weaponController.GetType().GetMethod("GetWeaponType");
                    if (method != null)
                    {
                        object result = method.Invoke(weaponController, new object[] { 
                            (WeaponStateActive)System.Enum.Parse(typeof(WeaponStateActive), "PrimaryActive") 
                        });
                        return result as WeaponBase;
                    }
                }
            }
            return null;
        }
        
        private WeaponBase GetCurrentSecondaryWeapon()
        {
            if (_weaponController == null) return null;
            
            System.Reflection.FieldInfo field = typeof(BossWeaponController).GetField("_weaponController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                object weaponController = field.GetValue(_weaponController);
                if (weaponController != null)
                {
                    System.Reflection.MethodInfo method = weaponController.GetType().GetMethod("GetWeaponType");
                    if (method != null)
                    {
                        object result = method.Invoke(weaponController, new object[] { 
                            (WeaponStateActive)System.Enum.Parse(typeof(WeaponStateActive), "SecondaryActive") 
                        });
                        return result as WeaponBase;
                    }
                }
            }
            return null;
        }
        
        private void ResetTimerSwitch()
        {
            _nextTimerSwitch = Random.Range(_minSwitchInterval, _maxSwitchInterval);
        }
        
        public void ForceSwitchNow()
        {
            if (CanSwitch())
            {
                PerformRandomSwitch();
                ResetTimerSwitch();
            }
        }
        
        public void ResetAllTriggers()
        {
            _hasTriggeredThresholdSwitch = false;
            _triggeredThresholds.Clear();
            _currentSwitchTimer = 0f;
            _isSwitching = false;
            ResetTimerSwitch();
        }
        
        public void AddHealthThreshold(float threshold)
        {
            if (!_additionalThresholds.Contains(threshold))
            {
                _additionalThresholds.Add(threshold);
                _additionalThresholds.Sort();
            }
        }
    }
}