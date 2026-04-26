using UnityEngine;
using Health;

namespace Enemy.Animation
{
    public class HealthAnimationSpeedController : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedMultiplierParam = "SpeedMultiplier";
        [SerializeField] private float _healthThreshold = 0.5f;
        [SerializeField] private float _lowHealthSpeed = 2f;
        
        private int _speedMultiplierHash;
        
        private void Awake()
        {
            _speedMultiplierHash = Animator.StringToHash(_speedMultiplierParam);
        }
        
        private void OnEnable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit += OnHealthChanged;
                _healthController.OnHeal += OnHealthChanged;
                _healthController.OnDeath += OnDeath;
            }
        }
        
        private void OnDisable()
        {
            if (_healthController != null)
            {
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
    }
}