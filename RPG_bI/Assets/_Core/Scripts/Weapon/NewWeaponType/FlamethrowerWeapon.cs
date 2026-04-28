using System.Collections.Generic;
using Damage;
using Health;
using UnityEngine;

namespace Weapons
{
    public class FlamethrowerWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Melee;
        [SerializeField] private float _range = 5f;
        [SerializeField] private float _coneAngle = 30f;
        [SerializeField] private int _rayCount = 8;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private GameObject _flameEffect;
        [SerializeField] private float _effectDuration = 0.3f;
        [SerializeField] private float _damageInterval = 0.2f;
        
        private bool _isAttacking;
        private float _damageTimer;
        
        public override EnemyType EnemyType => _enemyType;
        
        public override void Attack()
        {
            if (_isAttacking) return;
            
            _isAttacking = true;
            _damageTimer = 0f;
            
            ActivateEffect();
        }
        
        private void Update()
        {
            if (!_isAttacking) return;
            
            _damageTimer += Time.deltaTime;
            
            if (_damageTimer >= _damageInterval)
            {
                _damageTimer = 0f;
                ApplyDamageInCone();
            }
        }
        
        private void ApplyDamageInCone()
        {
            if (_attackPoint == null) return;
            
            float halfAngle = _coneAngle * 0.5f * Mathf.Deg2Rad;
            
            for (int i = 0; i <= _rayCount; i++)
            {
                float t = (float)i / _rayCount;
                float angle = -halfAngle + (t * _coneAngle * Mathf.Deg2Rad);
                
                Vector3 direction = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0) * _attackPoint.forward;
                
                if (Physics.Raycast(_attackPoint.position, direction, out RaycastHit hit, _range, _targetLayer))
                {
                    if (hit.collider.TryGetComponent<IDamageable>(out var healthController))
                    {
                        healthController.Damage(_damageDataSO.DamageList);
                    }
                }
            }
        }
        
        public void StopAttack()
        {
            _isAttacking = false;
            if (_flameEffect != null)
                _flameEffect.SetActive(false);
        }
        
        private void ActivateEffect()
        {
            if (_flameEffect != null)
            {
                StopAllCoroutines();
                StartCoroutine(ActivateEffectCoroutine());
            }
        }
        
        private System.Collections.IEnumerator ActivateEffectCoroutine()
        {
            _flameEffect.SetActive(true);
            yield return new WaitForSeconds(_effectDuration);
            _flameEffect.SetActive(false);
            _isAttacking = false;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_attackPoint == null) return;
            
            float halfAngle = _coneAngle * 0.5f;
            Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * _attackPoint.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * _attackPoint.forward;
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_attackPoint.position, leftBoundary * _range);
            Gizmos.DrawRay(_attackPoint.position, rightBoundary * _range);
            
            for (int i = 0; i <= _rayCount; i++)
            {
                float t = (float)i / _rayCount;
                float angle = -halfAngle + (t * _coneAngle);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * _attackPoint.forward;
                Gizmos.DrawRay(_attackPoint.position, direction * _range);
            }
        }
    }
}