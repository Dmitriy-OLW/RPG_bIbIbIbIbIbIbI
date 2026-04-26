using UnityEngine;

namespace Enemy.Navigation
{
    public class WeaponProjectileLookInstant : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AIVisionController _visionController;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _targetHeightOffset = 1f;
        
        private Quaternion _initialLocalRotation;

        private void Start()
        {
            if (_shootPoint == null)
                _shootPoint = transform;
                
            _initialLocalRotation = _shootPoint.localRotation;
        }

        private void Update()
        {
            if (_visionController == null || !_visionController.HasTarget)
            {
                ReturnToDefaultRotation();
                return;
            }

            RotateTowardsTarget();
        }

        private void RotateTowardsTarget()
        {
            Transform target = _visionController.CurrentTarget;
            if (target == null) return;
            
            Vector3 targetPosition = target.position + Vector3.up * _targetHeightOffset;
            Vector3 directionToTarget = targetPosition - _shootPoint.position;

            if (directionToTarget == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            if (_shootPoint.parent != null)
            {
                targetRotation = Quaternion.Inverse(_shootPoint.parent.rotation) * targetRotation;
            }
            
            _shootPoint.localRotation = targetRotation;
        }

        private void ReturnToDefaultRotation()
        {
            _shootPoint.localRotation = _initialLocalRotation;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_shootPoint == null) return;
            
            if (_visionController != null && _visionController.HasTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(_shootPoint.position, _shootPoint.forward * 5f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Vector3 forward = _shootPoint.parent != null ? 
                    _shootPoint.parent.forward : Vector3.forward;
                Gizmos.DrawRay(_shootPoint.position, forward * 5f);
            }
        }
    }
}