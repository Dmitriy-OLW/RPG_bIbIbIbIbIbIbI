using UnityEngine;
using System.Collections;

namespace Health
{
    public class DeathHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _deathScreenDelay = 3f;
        
        [SerializeField] private HealthController[] _healthControllers;
        [SerializeField] private GameObject _deathCanvas;
        
        [SerializeField] private MonoBehaviour[] _scriptsToDisable;
        [SerializeField] private GameObject[] _gameObjectsToDisable;
        
        private bool _isDead = false;

        private void Awake()
        {
            if (_deathCanvas != null)
                _deathCanvas.SetActive(false);
        }

        private void OnEnable()
        {
            SubscribeToHealthControllers();
        }

        private void OnDisable()
        {
            UnsubscribeFromHealthControllers();
        }

        private void SubscribeToHealthControllers()
        {
            foreach (HealthController healthController in _healthControllers)
            {
                if (healthController != null)
                    healthController.OnDeath += HandleDeath;
            }
        }

        private void UnsubscribeFromHealthControllers()
        {
            foreach (HealthController healthController in _healthControllers)
            {
                if (healthController != null) 
                    healthController.OnDeath -= HandleDeath;
            }
        }
        
        private void HandleDeath()
        {
            if (_isDead) return;
            
            _isDead = true;
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {           
            DisablePlayerControls();
            
            yield return new WaitForSeconds(_deathScreenDelay);

            Time.timeScale = 0f;
            
            if (_deathCanvas != null)
                _deathCanvas.SetActive(true);
        }

        private void DisablePlayerControls()
        {
            if (_scriptsToDisable != null)
            {
                foreach (var script in _scriptsToDisable)
                {
                    if (script != null)
                        script.enabled = false;
                }
            }
            
            if (_gameObjectsToDisable != null)
            {
                foreach (var obj in _gameObjectsToDisable)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }
    }
}