using System.Collections;
using System.Collections.Generic;
using Damage;
using Health;
using UnityEngine;

namespace Weapons
{
    public class MachineGunWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Ranged;
        [SerializeField] private float _range = 25f;
        [SerializeField] private float _coneAngle = 5f;
        [SerializeField] private float _burstDuration = 2f;
        [SerializeField] private float _shotInterval = 0.1f;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _effectObject;
        [SerializeField] private GameObject _muzzleFlashObject;
        [SerializeField] private float _effectDuration = 0.05f;
        
        private bool _isFiring;
        private Coroutine _firingCoroutine;
        
        public override EnemyType EnemyType => _enemyType;
        
        public override void Attack()
        {
            if (_shootPoint == null || _damageDataSO?.DamageList == null) return;
            if (_isFiring) return;
            
            if (_firingCoroutine != null)
                StopCoroutine(_firingCoroutine);
            
            _firingCoroutine = StartCoroutine(FireBurstCoroutine());
        }
        
        private IEnumerator FireBurstCoroutine()
        {
            _isFiring = true;
            ActivateEffect();
            
            float startTime = Time.time;
            
            while (Time.time - startTime < _burstDuration)
            {
                FireSingleShot();
                yield return new WaitForSeconds(_shotInterval);
            }
            
            _isFiring = false;
            DeactivateEffect();
        }
        
        private void FireSingleShot()
        {
            Vector3 spreadDirection = GetSpreadDirection(_shootPoint.forward);
            PerformRaycast(spreadDirection);
            ActivateMuzzleFlash();
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
        }
        
        private void ActivateMuzzleFlash()
        {
            if (_muzzleFlashObject != null)
            {
                _muzzleFlashObject.SetActive(true);
                StartCoroutine(DisableMuzzleFlash());
            }
        }
        
        private IEnumerator DisableMuzzleFlash()
        {
            yield return new WaitForSeconds(_effectDuration);
            if (_muzzleFlashObject != null)
                _muzzleFlashObject.SetActive(false);
        }
        
        private void ActivateEffect()
        {
            if (_effectObject != null)
            {
                _effectObject.SetActive(true);
            }
        }
        
        private void DeactivateEffect()
        {
            if (_effectObject != null)
            {
                _effectObject.SetActive(false);
            }
            
            if (_muzzleFlashObject != null)
            {
                _muzzleFlashObject.SetActive(false);
            }
        }
        
        public void StopFiring()
        {
            if (_firingCoroutine != null)
            {
                StopCoroutine(_firingCoroutine);
                _firingCoroutine = null;
            }
            _isFiring = false;
            DeactivateEffect();
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