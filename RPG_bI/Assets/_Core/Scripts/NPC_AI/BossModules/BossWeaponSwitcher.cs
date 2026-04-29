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
        private bool _hasReceivedFirstHit;
        
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
                _healthController.OnHit += OnFirstHitHandler;
            }
        }
        
        private void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit -= OnHitReceived;
                _healthController.OnHit -= OnFirstHitHandler;
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
            return _hasReceivedFirstHit && !_isSwitching && _currentSwitchTimer <= 0f && GetAvailableConfigCount() >= 2;
        }
        
        private int GetAvailableConfigCount()
        {
            if (_weaponController == null) return 0;
            
            int count = 0;
            for (int i = 0; i < _weaponController.GetConfigCount(); i++)
            {
                var config = _weaponController.GetConfig(i);
                if (config != null && (config.CanBePrimary || config.CanBeSecondary))
                    count++;
            }
            return count;
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
        
        private void OnFirstHitHandler()
        {
            if (!_hasReceivedFirstHit)
            {
                _hasReceivedFirstHit = true;
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
            if (_weaponController == null) return;
            
            _isSwitching = true;
            
            int primaryConfigIndex = -1;
            int secondaryConfigIndex = -1;
            
            if (_swapPrimaryAndSecondary)
            {
                int currentPrimaryIndex = GetCurrentPrimaryConfigIndex();
                int currentSecondaryIndex = GetCurrentSecondaryConfigIndex();
                
                if (currentPrimaryIndex >= 0 && currentSecondaryIndex >= 0 && 
                    CanBePrimary(currentSecondaryIndex) && CanBeSecondary(currentPrimaryIndex))
                {
                    primaryConfigIndex = currentSecondaryIndex;
                    secondaryConfigIndex = currentPrimaryIndex;
                }
            }
            
            if (primaryConfigIndex == -1 && _useRandomConfigForPrimary)
            {
                primaryConfigIndex = GetRandomValidConfigIndex(true);
            }
            
            if (secondaryConfigIndex == -1 && _useRandomConfigForSecondary)
            {
                secondaryConfigIndex = GetRandomValidConfigIndex(false);
                
                if (secondaryConfigIndex == primaryConfigIndex)
                {
                    int attempts = 0;
                    while (secondaryConfigIndex == primaryConfigIndex && attempts < 10)
                    {
                        secondaryConfigIndex = GetRandomValidConfigIndex(false);
                        attempts++;
                    }
                }
            }
            
            if (primaryConfigIndex >= 0 && CanBePrimary(primaryConfigIndex))
            {
                _weaponController.SetWeaponFromConfig(primaryConfigIndex, true);
            }
            
            if (secondaryConfigIndex >= 0 && secondaryConfigIndex != primaryConfigIndex && CanBeSecondary(secondaryConfigIndex))
            {
                _weaponController.SetWeaponFromConfig(secondaryConfigIndex, false);
            }
            
            _currentSwitchTimer = _globalSwitchCooldown;
            _isSwitching = false;
        }
        
        private bool CanBePrimary(int configIndex)
        {
            var config = _weaponController.GetConfig(configIndex);
            return config != null && config.CanBePrimary;
        }
        
        private bool CanBeSecondary(int configIndex)
        {
            var config = _weaponController.GetConfig(configIndex);
            return config != null && config.CanBeSecondary;
        }
        
        private int GetRandomValidConfigIndex(bool isPrimary)
        {
            List<int> validIndices = _weaponController.GetValidConfigIndices(isPrimary);
            if (validIndices.Count == 0) return -1;
            return validIndices[Random.Range(0, validIndices.Count)];
        }
        
        public void SwitchToSpecificConfigs(int primaryIndex, int secondaryIndex)
        {
            if (!CanSwitch()) return;
            
            _isSwitching = true;
            
            if (primaryIndex >= 0 && primaryIndex < _weaponController.GetConfigCount() && CanBePrimary(primaryIndex))
            {
                _weaponController.SetWeaponFromConfig(primaryIndex, true);
            }
            
            if (secondaryIndex >= 0 && secondaryIndex < _weaponController.GetConfigCount() && 
                secondaryIndex != primaryIndex && CanBeSecondary(secondaryIndex))
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
            
            int newPrimaryIndex = GetNextValidConfig(currentPrimaryIndex, true);
            int newSecondaryIndex = GetNextValidConfig(currentSecondaryIndex, false);
            
            if (newSecondaryIndex == newPrimaryIndex)
            {
                newSecondaryIndex = GetNextValidConfig(newSecondaryIndex, false);
            }
            
            SwitchToSpecificConfigs(newPrimaryIndex, newSecondaryIndex);
        }
        
        private int GetNextValidConfig(int currentIndex, bool isPrimary)
        {
            List<int> validIndices = _weaponController.GetValidConfigIndices(isPrimary);
            if (validIndices.Count == 0) return -1;
            
            int currentPos = validIndices.IndexOf(currentIndex);
            int nextPos = (currentPos + 1) % validIndices.Count;
            return validIndices[nextPos];
        }
        
        private int GetCurrentPrimaryConfigIndex()
        {
            WeaponBase currentWeapon = GetCurrentPrimaryWeapon();
            if (currentWeapon == null) return -1;
            
            for (int i = 0; i < _weaponController.GetConfigCount(); i++)
            {
                WeaponConfig config = _weaponController.GetConfig(i);
                if (config != null && config.Weapon == currentWeapon && config.CanBePrimary)
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
                if (config != null && config.Weapon == currentWeapon && config.CanBeSecondary)
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