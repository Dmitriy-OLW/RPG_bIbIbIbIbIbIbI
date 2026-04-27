using UnityEngine;
using Health;

namespace Score
{
    public class ScoreOnDeath : MonoBehaviour
    {
        [SerializeField] private int _pointsToAward = 10;
        private HealthController _healthController;
        
        private void OnEnable()
        {
            _healthController = GetComponent<HealthController>();
            if (_healthController != null)
            {
                _healthController.OnDeath += AwardScore;
            }
        }
        
        private void OnDisable()
        {
            if (_healthController != null)
            {
                _healthController.OnDeath -= AwardScore;
            }
        }
        
        private void AwardScore()
        {
            ScoreManager.AddScore(_pointsToAward);
        }
    }
}