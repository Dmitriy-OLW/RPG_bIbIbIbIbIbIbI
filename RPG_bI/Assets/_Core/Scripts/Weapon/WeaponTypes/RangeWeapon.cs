using UnityEngine;

namespace Weapons
{
    public class RangeWeapon : WeaponBase
    {
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _projectileSpeed = 20f;

        public override void Attack()
        {
            if (_projectilePrefab == null || _shootPoint == null || _damageDataSO.DamageList == null || _damageDataSO.DamageList.Count == 0)
                return;

            Projectile projectile = Instantiate(_projectilePrefab, _shootPoint.position, _shootPoint.rotation);
            projectile.Initialize(_damageDataSO.DamageList, _projectileSpeed);
        }
    }
}