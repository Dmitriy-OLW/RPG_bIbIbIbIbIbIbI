using System.Collections.Generic;
using Damage;
using Health;
using UnityEngine;

namespace Weapons
{
    public class PistolWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Ranged;
        [SerializeField] private float _range = 20f;
        [SerializeField] private float _coneAngle = 5f;
        [SerializeField] private int _pelletCount = 1;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _effectObject;
        [SerializeField] private float _effectDuration = 0.5f;
        
        public override EnemyType EnemyType => _enemyType;
        
        public override void Attack()
        {
            if (_shootPoint == null || _damageDataSO?.DamageList == null) return;
            
            Vector3 direction = _shootPoint.forward;
            
            for (int i = 0; i < _pelletCount; i++)
            {
                Vector3 spreadDirection = GetSpreadDirection(direction);
                PerformRaycast(spreadDirection);
            }
            
            ActivateEffect();
        }
        
        private Vector3 GetSpreadDirection(Vector3 baseDirection)
        {
            float randomYaw = Random.Range(-_coneAngle, _coneAngle) * Mathf.Deg2Rad;
            float randomPitch = Random.Range(-_coneAngle, _coneAngle) * Mathf.Deg2Rad;
            
            Quaternion rotation = Quaternion.LookRotation(baseDirection);
            Vector3 spreadOffset = new Vector3(randomYaw, randomPitch, 0);
            
            return rotation * Quaternion.Euler(spreadOffset) * Vector3.forward;
        }
        
        private void PerformRaycast(Vector3 direction)
        {
            if (Physics.Raycast(_shootPoint.position, direction, out RaycastHit hit, _range, _targetLayer))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var healthController))
                {
                    healthController.Damage(_damageDataSO.DamageList);
                }
            }
            
            Debug.DrawRay(_shootPoint.position, direction * _range, Color.red, 0.1f);
        }
        
        private void ActivateEffect()
        {
            if (_effectObject != null)
            {
                StopAllCoroutines();
                StartCoroutine(ActivateEffectCoroutine());
            }
        }
        
        private System.Collections.IEnumerator ActivateEffectCoroutine()
        {
            _effectObject.SetActive(true);
            yield return new WaitForSeconds(_effectDuration);
            _effectObject.SetActive(false);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_shootPoint == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_shootPoint.position, _shootPoint.forward * _range);
            
            Vector3 leftBoundary = Quaternion.Euler(0, -_coneAngle, 0) * _shootPoint.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, _coneAngle, 0) * _shootPoint.forward;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_shootPoint.position, leftBoundary * _range);
            Gizmos.DrawRay(_shootPoint.position, rightBoundary * _range);
        }
    }
}