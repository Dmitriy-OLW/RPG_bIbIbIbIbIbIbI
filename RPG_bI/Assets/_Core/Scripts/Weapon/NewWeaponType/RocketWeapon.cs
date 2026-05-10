using UnityEngine;

namespace Weapons
{
    public class RocketWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Ranged;
        [SerializeField] private RocketProjectile _rocketPrefab;
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private float _rocketSpeed = 25f;
        [SerializeField] private GameObject _launchEffect;        
        [SerializeField] private float _launchEffectDuration = 3f; 
        
        public override EnemyType EnemyType => _enemyType;
        
        public override void Attack()
        {
            if (_rocketPrefab == null || _launchPoint == null) return;
            
            RocketProjectile rocket = Instantiate(_rocketPrefab, _launchPoint.position, _launchPoint.rotation);
            rocket.Initialize(_damageDataSO.DamageList, _rocketSpeed);
            
            if (_launchEffect != null)
            {
                GameObject effect = Instantiate(_launchEffect, _launchPoint.position, _launchPoint.rotation);
                Destroy(effect, _launchEffectDuration);
            }
        }
    }
}