using UnityEngine;
using UnityEngine.UI;

namespace RPGbI.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health.HealthController _healthController;
        [SerializeField] private Image _fillImage;
        
        private void OnEnable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit += UpdateHealthBar;
                _healthController.OnDeath += UpdateHealthBar;
            }
        }

        private void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.OnHit -= UpdateHealthBar;
                _healthController.OnDeath -= UpdateHealthBar;
            }
        }

        private void UpdateHealthBar()
        {
            if (_fillImage != null && _healthController != null)
            {
                _fillImage.fillAmount = _healthController.HealthPercentage;
            }
        }
    }
}