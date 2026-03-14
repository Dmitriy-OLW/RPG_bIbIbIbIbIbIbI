using UnityEngine;

namespace Enemy.Navigation
{
    public class WeaponProjectileLook : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AIVisionController _visionController;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _maxHorizontalAngle = 20f;
        [SerializeField] private float _maxVerticalAngle = 20f;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _targetHeightOffset = 1f;
        
        private Quaternion _initialLocalRotation;
        private Quaternion _targetLocalRotation;

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
            
            Vector3 limitedAngles = LimitAngles(targetRotation.eulerAngles);
            _targetLocalRotation = Quaternion.Euler(limitedAngles);
            
            _shootPoint.localRotation = Quaternion.Slerp(
                _shootPoint.localRotation, 
                _targetLocalRotation, 
                Time.deltaTime * _rotationSpeed
            );
        }

        private Vector3 LimitAngles(Vector3 angles)
        {
            if (angles.x > 180) angles.x -= 360;
            if (angles.y > 180) angles.y -= 360;
            if (angles.z > 180) angles.z -= 360;
            
            Vector3 initialAngles = _initialLocalRotation.eulerAngles;
            if (initialAngles.x > 180) initialAngles.x -= 360;
            if (initialAngles.y > 180) initialAngles.y -= 360;
            if (initialAngles.z > 180) initialAngles.z -= 360;
            
            float minX = initialAngles.x - _maxVerticalAngle;
            float maxX = initialAngles.x + _maxVerticalAngle;
            angles.x = Mathf.Clamp(angles.x, minX, maxX);
            
            float minY = initialAngles.y - _maxHorizontalAngle;
            float maxY = initialAngles.y + _maxHorizontalAngle;
            angles.y = Mathf.Clamp(angles.y, minY, maxY);
            
            angles.z = initialAngles.z;

            return angles;
        }

        private void ReturnToDefaultRotation()
        {
            _shootPoint.localRotation = Quaternion.Slerp(
                _shootPoint.localRotation, 
                _initialLocalRotation, 
                Time.deltaTime * _rotationSpeed
            );
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_shootPoint == null) return;
            
            Vector3 forward = _shootPoint.parent != null ? 
                _shootPoint.parent.forward : Vector3.forward;
            Vector3 right = _shootPoint.parent != null ? 
                _shootPoint.parent.right : Vector3.right;
            Vector3 up = _shootPoint.parent != null ? 
                _shootPoint.parent.up : Vector3.up;
            
            Gizmos.color = Color.yellow;
            float radius = 5f;
            
            Quaternion horizontalRotLeft = Quaternion.AngleAxis(-_maxHorizontalAngle, up);
            Quaternion horizontalRotRight = Quaternion.AngleAxis(_maxHorizontalAngle, up);
            
            Vector3 leftDir = horizontalRotLeft * forward;
            Vector3 rightDir = horizontalRotRight * forward;
            
            Gizmos.DrawRay(_shootPoint.position, leftDir * radius);
            Gizmos.DrawRay(_shootPoint.position, rightDir * radius);
            
            Gizmos.color = Color.cyan;
            
            Quaternion verticalRotDown = Quaternion.AngleAxis(-_maxVerticalAngle, right);
            Quaternion verticalRotUp = Quaternion.AngleAxis(_maxVerticalAngle, right);
            
            Vector3 downDir = verticalRotDown * forward;
            Vector3 upDir = verticalRotUp * forward;
            
            Gizmos.DrawRay(_shootPoint.position, downDir * radius);
            Gizmos.DrawRay(_shootPoint.position, upDir * radius);
            
            if (_visionController != null && _visionController.HasTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(_shootPoint.position, 
                    _shootPoint.forward * radius);
            }
        }
    }
}