using System.Collections.Generic;
using Damage;
using Health;
using UnityEngine;

namespace Weapons
{
    public class GrenadeProjectile : MonoBehaviour
    {
        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private float _explosionDelay = 5f;
        [SerializeField] private float _gravityScale = 1f;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private GameObject _explosionEffect;
        [SerializeField] private float _effectDuration = 1f;
        [SerializeField] private GameObject _impactEffect;       
        [SerializeField] private float _impactEffectDuration = 2f; 
        
        private List<DamageData> _damageDataList;
        private Rigidbody _rb;
        private bool _hasExploded;
        
        public void Initialize(List<DamageData> damageDataList)
        {
            _damageDataList = damageDataList;
            _rb = GetComponent<Rigidbody>();
            
            if (_rb != null)
            {
                _rb.useGravity = true;
                _rb.linearDamping = 0.5f;
            }
            
            Invoke(nameof(Explode), _explosionDelay);
        }
        
        private void Update()
        {
            if (_rb != null && !_hasExploded)
            {
                _rb.AddForce(Physics.gravity * _gravityScale, ForceMode.Acceleration);
            }
        }
        
        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius, _targetLayer);
            
            foreach (Collider hit in hitColliders)
            {
                if (hit.TryGetComponent<IDamageable>(out var healthController))
                {
                    healthController.Damage(_damageDataList);
                }
            }
            
            if (_explosionEffect != null)
            {
                GameObject effect = Instantiate(_explosionEffect, transform.position, Quaternion.identity);
                Destroy(effect, _effectDuration);
            }
            
            Destroy(gameObject);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!_hasExploded && collision.relativeVelocity.magnitude > 3f)
            {
                SpawnImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
                Explode();
            }
        }
        
        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (_impactEffect != null)
            {
                Quaternion rotation = Quaternion.LookRotation(normal);
                GameObject impactEffectInstance = Instantiate(_impactEffect, position, rotation);
                Destroy(impactEffectInstance, _impactEffectDuration);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}