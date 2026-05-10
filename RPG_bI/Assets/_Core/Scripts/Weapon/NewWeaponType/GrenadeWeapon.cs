using UnityEngine;

namespace Weapons
{
    public class GrenadeWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Ranged;
        [SerializeField] private GrenadeProjectile _grenadePrefab;
        [SerializeField] private Transform _throwPoint;
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _upwardForce = 5f;
        [SerializeField] private GameObject _launchEffect;       
        [SerializeField] private float _launchEffectDuration = 1f; 
        
        public override EnemyType EnemyType => _enemyType;
        
        public override void Attack()
        {
            if (_grenadePrefab == null || _throwPoint == null) return;
            
            GrenadeProjectile grenade = Instantiate(_grenadePrefab, _throwPoint.position, Quaternion.identity);
            grenade.Initialize(_damageDataSO.DamageList);
            
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 force = _throwPoint.forward * _throwForce + Vector3.up * _upwardForce;
                rb.AddForce(force, ForceMode.Impulse);
            }
            
            if (_launchEffect != null)
            {
                GameObject effect = Instantiate(_launchEffect, _throwPoint.position, _throwPoint.rotation);
                Destroy(effect, _launchEffectDuration);
            }
        }
    }
}