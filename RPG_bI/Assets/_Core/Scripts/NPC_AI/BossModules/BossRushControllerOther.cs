using UnityEngine;
using Health;

namespace Enemy.Animation
{
    public class BossRushControllerOther : MonoBehaviour
    {
        [SerializeField] private GameObject _objectToActivateOnFirstHit;
        [SerializeField] private HealthController _healthController;
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedMultiplierParam = "SpeedMultiplier";
        [SerializeField] private float _healthThreshold = 0.5f;
        [SerializeField] private float _lowHealthSpeed = 2f;
        
        private int _speedMultiplierHash;
        private bool _hasActivated;
        
        private void Awake()
        {
            _speedMultiplierHash = Animator.StringToHash(_speedMultiplierParam);
            
            if (_objectToActivateOnFirstHit != null)
            {
                _objectToActivateOnFirstHit.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit += OnFirstHit;
                _healthController.OnHit += OnHealthChanged;
                _healthController.OnHeal += OnHealthChanged;
                _healthController.OnDeath += OnDeath;
            }
        }
        
        private void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit -= OnFirstHit;
                _healthController.OnHit -= OnHealthChanged;
                _healthController.OnHeal -= OnHealthChanged;
                _healthController.OnDeath -= OnDeath;
            }
        }
        
        private void OnHealthChanged()
        {
            if (_healthController == null || _animator == null)
                return;
            
            float speed = _healthController.HealthPercentage <= _healthThreshold ? _lowHealthSpeed : 1f;
            _animator.SetFloat(_speedMultiplierHash, speed);
        }
        
        private void OnDeath()
        {
            if (_animator != null)
                _animator.SetFloat(_speedMultiplierHash, 1f);
        }
        
        private void OnFirstHit()
        {
            if (_hasActivated)
                return;
            
            _hasActivated = true;
            
            if (_objectToActivateOnFirstHit != null)
            {
                _objectToActivateOnFirstHit.SetActive(true);
            }
            
            _healthController.OnHit -= OnFirstHit;
        }
    }
}