using Health;
using UnityEngine;

namespace Weapons
{
    public class MeleeWeapon : WeaponBase
    {
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Vector3 _attackBoxSize = Vector3.one;
        [SerializeField] private Transform _attackPoint;

        public override void Attack()
        {
            if (_attackPoint == null || _damageDataSO.DamageList == null || _damageDataSO.DamageList.Count == 0)
                return;

            Collider[] hitColliders = Physics.OverlapBox(
                _attackPoint.position, 
                _attackBoxSize * 0.5f, 
                _attackPoint.rotation, 
                _targetLayer
            );
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var healthController))
                {
                    healthController.Damage(_damageDataSO.DamageList);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_attackPoint == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(_attackPoint.position, _attackPoint.rotation, _attackBoxSize);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}