using UnityEngine;
using Character.Targeting;
using System;

namespace Enemy.Navigation
{
    public class AIVisionController : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float _viewRadius = 15f;
        [SerializeField] private float _viewAngle = 90f;
        [SerializeField] private float _peripheralRadius = 3f;
        [SerializeField] private float _targetHeightOffset = 1.5f;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private Transform _visionPoint;
        [SerializeField] private Targeting _targetingComponent;
        
        private Transform _currentTarget;
        private float _distanceToTarget;

        public Action<Transform> OnTargetDetected;
        
        public Transform CurrentTarget => _currentTarget;
        public float DistanceToTarget => _distanceToTarget;
        public bool HasTarget => _currentTarget != null;

        private void Update()
        {
            FindVisibleTarget();
        }

        private void FindVisibleTarget()
        {
            if (_targetingComponent == null)
            {
                ClearTarget();
                return;
            }

            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            Collider[] targetsInRadius = Physics.OverlapSphere(_visionPoint.position, _viewRadius, _targetMask);
            
            foreach (Collider target in targetsInRadius)
            {
                Transform targetTransform = target.transform;
                Targeting targetFaction = targetTransform.GetComponent<Targeting>();
                
                if (targetFaction == null) 
                    continue;
                    
                if (!_targetingComponent.IsHostileTowards(targetFaction))
                    continue;
                
                if (CanSeeTarget(targetTransform, out float distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetTransform;
                    }
                }
            }
            
            if (closestTarget != null)
            {
                SetTarget(closestTarget, closestDistance);
            }
            else
            {
                ClearTarget();
            }
        }
        
        private bool CanSeeTarget(Transform target, out float distance)
        {
            distance = float.MaxValue;
            
            if (target == null)
                return false;
        
            Vector3 targetPosition = target.position + Vector3.up * _targetHeightOffset;
            Vector3 directionToTarget = (targetPosition - _visionPoint.position).normalized;
            distance = Vector3.Distance(_visionPoint.position, targetPosition);
            
            bool inPeripheralRange = distance <= _peripheralRadius;
            bool inCone = Vector3.Angle(_visionPoint.forward, directionToTarget) < _viewAngle / 2;
            
            if (!inPeripheralRange && !inCone)
                return false;
            
            if (HasClearLineOfSight(target, distance))
                return false;
                
            return true;
        }

        private bool HasClearLineOfSight(Transform target, float targetDistance)
        {
            Vector3 origin = _visionPoint.position;
            Vector3 targetPos = target.position + Vector3.up * _targetHeightOffset;
            
            Vector3 rightOffset = _visionPoint.right * 1f;
            Vector3 leftOffset = -_visionPoint.right * 1f;
            
            bool centerHit = Physics.Raycast(origin, (targetPos - origin).normalized, targetDistance, _obstacleMask);
            bool rightHit = Physics.Raycast(origin + rightOffset, (targetPos - (origin + rightOffset)).normalized, targetDistance, _obstacleMask);
            bool leftHit = Physics.Raycast(origin + leftOffset, (targetPos - (origin + leftOffset)).normalized, targetDistance, _obstacleMask);
            
            return (centerHit && rightHit && leftHit);
        }

        private void SetTarget(Transform target, float distance)
        {
            if (_currentTarget != target)
            {
                _currentTarget = target;
                OnTargetDetected?.Invoke(target);
            }
            _distanceToTarget = distance;
        }
        
        private void ClearTarget()
        {
            _currentTarget = null;
            _distanceToTarget = float.MaxValue;
        }
        
        public bool IsTargetInRange(float range)
        {
            return HasTarget && _distanceToTarget <= range;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_visionPoint == null) return;
            
            Gizmos.color = new Color(1, 1, 1, 0.1f);
            Gizmos.DrawSphere(_visionPoint.position, _viewRadius);
            
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.DrawWireArc(_visionPoint.position, Vector3.up, Vector3.forward, 360, _viewRadius);
            
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(_visionPoint.position, _peripheralRadius);
            
            Vector3 viewAngle01 = DirectionFromAngle(_visionPoint.eulerAngles.y, -_viewAngle / 2);
            Vector3 viewAngle02 = DirectionFromAngle(_visionPoint.eulerAngles.y, _viewAngle / 2);
    
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawLine(_visionPoint.position, _visionPoint.position + viewAngle01 * _viewRadius);
            UnityEditor.Handles.DrawLine(_visionPoint.position, _visionPoint.position + viewAngle02 * _viewRadius);
            
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(_visionPoint.position, Vector3.up, viewAngle01, _viewAngle, _viewRadius);
            
            Collider[] targetsInRadius = Physics.OverlapSphere(_visionPoint.position, _viewRadius, _targetMask);
            
            foreach (Collider target in targetsInRadius)
            {
                Transform targetTransform = target.transform;
                Targeting targetFaction = targetTransform.GetComponent<Targeting>();
                
                if (targetFaction == null) 
                    continue;
                    
                if (!_targetingComponent.IsHostileTowards(targetFaction))
                    continue;
                
                Vector3 targetPosition = targetTransform.position + Vector3.up * _targetHeightOffset;
                Vector3 directionToTarget = (targetPosition - _visionPoint.position).normalized;
                float distanceToTarget = Vector3.Distance(_visionPoint.position, targetPosition);
                
                bool inPeripheral = distanceToTarget <= _peripheralRadius;
                bool inCone = Vector3.Angle(_visionPoint.forward, directionToTarget) < _viewAngle / 2;
                
                if (inPeripheral || inCone)
                {
                    Vector3 rightOffset = _visionPoint.right * 1f;
                    Vector3 leftOffset = -_visionPoint.right * 1f;
                    
                    bool centerHit = Physics.Raycast(_visionPoint.position, directionToTarget, distanceToTarget, _obstacleMask);
                    bool rightHit = Physics.Raycast(_visionPoint.position + rightOffset, (targetPosition - (_visionPoint.position + rightOffset)).normalized, distanceToTarget, _obstacleMask);
                    bool leftHit = Physics.Raycast(_visionPoint.position + leftOffset, (targetPosition - (_visionPoint.position + leftOffset)).normalized, distanceToTarget, _obstacleMask);
                    
                    bool allThreeBlocked = centerHit && rightHit && leftHit;
                    
                    if (allThreeBlocked)
                    {
                        Gizmos.color = Color.red;
                        DrawDashedLine(_visionPoint.position, targetPosition, 0.5f);
                        
                        if (Physics.Raycast(_visionPoint.position, directionToTarget, out RaycastHit hit, distanceToTarget, _obstacleMask))
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            Gizmos.DrawSphere(hit.point, 0.3f);
                            
                            DrawArrow(_visionPoint.position, hit.point, Color.red);
                            
                            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
                            DrawDashedLine(hit.point, targetPosition, 0.3f);
                        }
                        
                        if (rightHit)
                        {
                            Vector3 rightOrigin = _visionPoint.position + rightOffset;
                            if (Physics.Raycast(rightOrigin, (targetPosition - rightOrigin).normalized, out hit, distanceToTarget, _obstacleMask))
                            {
                                Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
                                Gizmos.DrawSphere(hit.point, 0.2f);
                            }
                        }
                        
                        if (leftHit)
                        {
                            Vector3 leftOrigin = _visionPoint.position + leftOffset;
                            if (Physics.Raycast(leftOrigin, (targetPosition - leftOrigin).normalized, out hit, distanceToTarget, _obstacleMask))
                            {
                                Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
                                Gizmos.DrawSphere(hit.point, 0.2f);
                            }
                        }
                    }
                    else
                    {
                        if (targetTransform == _currentTarget)
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(_visionPoint.position, targetPosition);
                            
                            Gizmos.color = new Color(0, 1, 0, 0.3f);
                            Gizmos.DrawSphere(targetPosition, 0.5f);
                            
                            Vector3 rightOrigin = _visionPoint.position + rightOffset;
                            Vector3 leftOrigin = _visionPoint.position + leftOffset;
                            
                            if (!centerHit)
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(_visionPoint.position, targetPosition);
                            }
                            
                            if (!rightHit)
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(rightOrigin, targetPosition);
                            }
                            
                            if (!leftHit)
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(leftOrigin, targetPosition);
                            }
                        }
                        else
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(_visionPoint.position, targetPosition);
                            
                            Gizmos.color = new Color(1, 1, 0, 0.3f);
                            Gizmos.DrawSphere(targetPosition, 0.5f);
                        }
                    }
                }
                else
                {
                    Gizmos.color = Color.gray;
                    DrawDashedLine(_visionPoint.position, targetPosition, 0.5f);
                }
            }
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_visionPoint.position, 0.2f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_visionPoint.position, _visionPoint.forward * 2f);
            
            Vector3 rightPoint = _visionPoint.position + _visionPoint.right * 1f;
            Vector3 leftPoint = _visionPoint.position - _visionPoint.right * 1f;
            
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawSphere(rightPoint, 0.1f);
            Gizmos.DrawSphere(leftPoint, 0.1f);
            
            Gizmos.color = new Color(1, 0, 1, 0.5f);
            Gizmos.DrawSphere(_visionPoint.position + Vector3.up * _targetHeightOffset, 0.1f);
        }

        private void DrawArrow(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);
            
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
            
            Vector3 arrowPos = from + direction * (distance * 0.7f);
            float arrowLength = 0.3f;
            float arrowAngle = 20f;
            
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowAngle, 0) * Vector3.forward;
            
            Gizmos.DrawRay(arrowPos, right * arrowLength);
            Gizmos.DrawRay(arrowPos, left * arrowLength);
        }

        private void DrawDashedLine(Vector3 start, Vector3 end, float dashLength)
        {
            float distance = Vector3.Distance(start, end);
            int dashCount = Mathf.FloorToInt(distance / dashLength);
            
            for (int i = 0; i < dashCount; i += 2)
            {
                float t1 = (float)i / dashCount;
                float t2 = Mathf.Min((float)(i + 1) / dashCount, 1f);
                
                Vector3 point1 = Vector3.Lerp(start, end, t1);
                Vector3 point2 = Vector3.Lerp(start, end, t2);
                
                Gizmos.DrawLine(point1, point2);
            }
        }

        private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
        {
            angleInDegrees += eulerY;
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
#endif
    }
}