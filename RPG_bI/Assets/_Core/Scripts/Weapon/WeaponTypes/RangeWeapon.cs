using UnityEngine;

namespace Weapons
{
    public class RangeWeapon : WeaponBase
    {
        [SerializeField] private EnemyType _enemyType = EnemyType.Ranged;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _projectileSpeed = 20f;
        [SerializeField] private float _chargeTime = 10f;

        private float _currentChargeTimer = 10;
        private bool _isCharged;
        public float CurrentChargeProgress => _currentChargeTimer / _chargeTime;
        public override EnemyType EnemyType => _enemyType;
        
        private void FixedUpdate()
        {
            if (_isCharged) return;

            _currentChargeTimer += Time.deltaTime;
            
            if (_currentChargeTimer >= _chargeTime)
                _isCharged = true;
        }
        
        public override void Attack()
        {
            if (CanAttack() == false) return;
            
            Projectile projectile = Instantiate(_projectilePrefab, _shootPoint.position, _shootPoint.rotation);
            projectile.Initialize(_damageDataSO.DamageList, _projectileSpeed);
            
            _isCharged = false;
            _currentChargeTimer = 0f;
        }
        
        private bool CanAttack()
        {
            if (_projectilePrefab == null) return false;
            if (_shootPoint == null) return false;
            if (_damageDataSO?.DamageList == null) return false;
            if (_damageDataSO.DamageList.Count == 0) return false;
            if (_isCharged == false) return false;
            
            return true;
        }
    }
}