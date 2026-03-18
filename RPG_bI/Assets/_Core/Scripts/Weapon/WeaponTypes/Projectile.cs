using System.Collections.Generic;
using Damage;
using Health;
using UnityEngine;

namespace Weapons
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 5f;
        
        private List<DamageData> _damageDataList;
        private float _speed;
        private float _currentLifeTime;

        public void Initialize(List<DamageData> damageDataList, float speed)
        {
            _damageDataList = damageDataList;
            _speed = speed;
        }
        
        private void OnEnable()
        {
            _currentLifeTime = 0f;
        }

        private void Update()
        {
            MoveProjectile();
            UpdateLifeTime();
        }

        private void MoveProjectile()
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

        private void UpdateLifeTime()
        {
            _currentLifeTime += Time.deltaTime;
            
            if (_currentLifeTime >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out var healthController))
            {
                healthController.Damage(_damageDataList);
                Destroy(gameObject);
            }
        }
    }
}