using UnityEngine;

namespace Player
{
    public class PlayerWeaponLook : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private Transform _lookTransform;
        [SerializeField] private LayerMask _entityLayer;
        [SerializeField] private float _detectionRadius = 10f;
        [SerializeField] private float _maxHorizontalAngle = 45f;
        [SerializeField] private float _maxVerticalAngle = 30f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _targetHeightOffset = 1f;
        [SerializeField] private bool _useSmoothRotation = true;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;
        [SerializeField] private Color _coneColor = Color.yellow;
        [SerializeField] private Color _verticalConeColor = Color.cyan;
        
        private Quaternion _initialLocalRotation;
        private Quaternion _targetLocalRotation;
        private Transform _currentTarget;
        private bool _hasTarget;
        
        public Transform CurrentTarget => _currentTarget;
        public bool HasTarget => _hasTarget;

        private void Start()
        {
            if (_lookTransform == null)
                _lookTransform = transform;
                
            _initialLocalRotation = _lookTransform.localRotation;
            
            // Выводим отладочную информацию о направлении
            Debug.Log($"LookTransform forward direction: {_lookTransform.forward}");
            Debug.Log($"LookTransform local forward: {_lookTransform.InverseTransformDirection(Vector3.forward)}");
        }

        private void Update()
        {
            DetectEntities();
            
            if (_hasTarget && _currentTarget != null)
            {
                RotateTowardsTarget();
            }
            else
            {
                ReturnToDefaultRotation();
            }
        }

        private void DetectEntities()
        {
            Collider[] colliders = Physics.OverlapSphere(
                _lookTransform.position, 
                _detectionRadius, 
                _entityLayer
            );
            
            Transform bestTarget = null;
            float bestAngle = float.MaxValue;
            
            // Получаем правильное направление "вперед" для объекта
            Vector3 forwardDirection = _lookTransform.forward;
            Vector3 upDirection = _lookTransform.up;
            Vector3 rightDirection = _lookTransform.right;
            
            foreach (Collider col in colliders)
            {
                if (col.transform == transform || col.transform == _lookTransform) continue;
                
                Vector3 directionToEntity = col.transform.position - _lookTransform.position;
                float distance = directionToEntity.magnitude;
                
                if (distance <= 0) continue;
                
                // Нормализуем направление
                Vector3 normalizedDirection = directionToEntity.normalized;
                
                // Вычисляем горизонтальный угол (отклонение влево-вправо)
                Vector3 horizontalProjection = Vector3.ProjectOnPlane(normalizedDirection, upDirection).normalized;
                float horizontalAngle = Vector3.Angle(forwardDirection, horizontalProjection);
                
                // Определяем знак горизонтального угла (влево или вправо)
                Vector3 crossHorizontal = Vector3.Cross(forwardDirection, horizontalProjection);
                if (Vector3.Dot(crossHorizontal, upDirection) < 0)
                    horizontalAngle = -horizontalAngle;
                
                // Вычисляем вертикальный угол (отклонение вверх-вниз)
                Vector3 verticalProjection = Vector3.ProjectOnPlane(normalizedDirection, rightDirection).normalized;
                float verticalAngle = Vector3.Angle(forwardDirection, verticalProjection);
                
                // Определяем знак вертикального угла (вверх или вниз)
                Vector3 crossVertical = Vector3.Cross(forwardDirection, verticalProjection);
                if (Vector3.Dot(crossVertical, rightDirection) > 0)
                    verticalAngle = -verticalAngle;
                
                // Проверяем, находится ли цель в конусе
                if (Mathf.Abs(horizontalAngle) <= _maxHorizontalAngle && 
                    Mathf.Abs(verticalAngle) <= _maxVerticalAngle)
                {
                    float totalAngle = Mathf.Abs(horizontalAngle) + Mathf.Abs(verticalAngle);
                    
                    if (bestTarget == null || totalAngle < bestAngle)
                    {
                        bestTarget = col.transform;
                        bestAngle = totalAngle;
                    }
                }
            }
            
            if (bestTarget != null)
            {
                _currentTarget = bestTarget;
                _hasTarget = true;
            }
            else
            {
                _currentTarget = null;
                _hasTarget = false;
            }
        }

        private void RotateTowardsTarget()
        {
            if (_currentTarget == null)
            {
                _hasTarget = false;
                return;
            }
            
            Vector3 targetPosition = _currentTarget.position + Vector3.up * _targetHeightOffset;
            Vector3 directionToTarget = targetPosition - _lookTransform.position;

            if (directionToTarget == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, _lookTransform.up);
            
            if (_lookTransform.parent != null)
            {
                targetRotation = Quaternion.Inverse(_lookTransform.parent.rotation) * targetRotation;
            }
            
            Vector3 limitedAngles = LimitAngles(targetRotation.eulerAngles);
            _targetLocalRotation = Quaternion.Euler(limitedAngles);
            
            if (_useSmoothRotation)
            {
                _lookTransform.localRotation = Quaternion.Slerp(
                    _lookTransform.localRotation, 
                    _targetLocalRotation, 
                    Time.deltaTime * _rotationSpeed
                );
            }
            else
            {
                _lookTransform.localRotation = Quaternion.RotateTowards(
                    _lookTransform.localRotation,
                    _targetLocalRotation,
                    _rotationSpeed * Time.deltaTime * 100f
                );
            }
        }

        private Vector3 LimitAngles(Vector3 angles)
        {
            // Нормализуем углы
            if (angles.x > 180) angles.x -= 360;
            if (angles.y > 180) angles.y -= 360;
            if (angles.z > 180) angles.z -= 360;
            
            Vector3 initialAngles = _initialLocalRotation.eulerAngles;
            if (initialAngles.x > 180) initialAngles.x -= 360;
            if (initialAngles.y > 180) initialAngles.y -= 360;
            if (initialAngles.z > 180) initialAngles.z -= 360;
            
            // Ограничиваем углы
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
            _lookTransform.localRotation = Quaternion.Slerp(
                _lookTransform.localRotation, 
                _initialLocalRotation, 
                Time.deltaTime * _rotationSpeed
            );
        }
        
        public void SetTarget(Transform target)
        {
            _currentTarget = target;
            _hasTarget = target != null;
        }
        
        public void ClearTarget()
        {
            _currentTarget = null;
            _hasTarget = false;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos || _lookTransform == null) return;
            
            // Получаем направления объекта
            Vector3 forward = _lookTransform.forward;
            Vector3 right = _lookTransform.right;
            Vector3 up = _lookTransform.up;
            
            // Рисуем сферу обнаружения
            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawSphere(_lookTransform.position, _detectionRadius);
            
            // Рисуем центральную ось (всегда вперед объекта)
            Gizmos.color = Color.white;
            Gizmos.DrawRay(_lookTransform.position, forward * _detectionRadius);
            
            // Горизонтальные границы конуса (влево-вправо)
            Gizmos.color = _coneColor;
            
            Quaternion horizontalRotLeft = Quaternion.AngleAxis(-_maxHorizontalAngle, up);
            Quaternion horizontalRotRight = Quaternion.AngleAxis(_maxHorizontalAngle, up);
            
            Vector3 leftDir = horizontalRotLeft * forward;
            Vector3 rightDir = horizontalRotRight * forward;
            
            Gizmos.DrawRay(_lookTransform.position, leftDir * _detectionRadius);
            Gizmos.DrawRay(_lookTransform.position, rightDir * _detectionRadius);
            
            // Рисуем горизонтальную дугу
            DrawArcInPlane(forward, up, _maxHorizontalAngle, 30, _coneColor);
            
            // Вертикальные границы конуса (вверх-вниз)
            Gizmos.color = _verticalConeColor;
            
            Quaternion verticalRotUp = Quaternion.AngleAxis(_maxVerticalAngle, right);
            Quaternion verticalRotDown = Quaternion.AngleAxis(-_maxVerticalAngle, right);
            
            Vector3 upDir = verticalRotUp * forward;
            Vector3 downDir = verticalRotDown * forward;
            
            Gizmos.DrawRay(_lookTransform.position, upDir * _detectionRadius);
            Gizmos.DrawRay(_lookTransform.position, downDir * _detectionRadius);
            
            // Рисуем вертикальную дугу
            DrawArcInPlane(forward, right, _maxVerticalAngle, 30, _verticalConeColor);
            
            // Рисуем оси координат для отладки
            if (_showDebugGizmos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_lookTransform.position, right * 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(_lookTransform.position, up * 0.5f);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_lookTransform.position, forward * 0.5f);
            }
            
            // Текущая цель
            if (_hasTarget && _currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_lookTransform.position, 
                    _currentTarget.position + Vector3.up * _targetHeightOffset);
            }
        }
        
        private void DrawArcInPlane(Vector3 forward, Vector3 rotationAxis, float maxAngle, int segments, Color color)
        {
            Gizmos.color = color;
            Vector3 previousPoint = _lookTransform.position + 
                Quaternion.AngleAxis(-maxAngle, rotationAxis) * forward * _detectionRadius;
            
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = Mathf.Lerp(-maxAngle, maxAngle, (float)i / segments);
                Vector3 currentPoint = _lookTransform.position + 
                    Quaternion.AngleAxis(currentAngle, rotationAxis) * forward * _detectionRadius;
                
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}