using Health;
using UnityEngine;

namespace Weapons
{
    public class MeleeWeapon : WeaponBase
    {
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private Transform _attackPoint;

        public override void Attack()
        {
            if (_attackPoint == null || _damageDataList == null || _damageDataList.Count == 0)
                return;

            Collider[] hitColliders = Physics.OverlapSphere(_attackPoint.position, _attackRange, _targetLayer);
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var healthController))
                {
                    healthController.Damage(_damageDataList);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_attackPoint == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _attackRange);
        }
    }
}