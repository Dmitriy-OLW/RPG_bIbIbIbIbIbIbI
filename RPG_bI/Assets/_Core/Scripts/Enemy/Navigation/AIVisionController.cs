using UnityEngine;
using System;
using Character.Targeting;
using System.Collections.Generic;

namespace Enemy.Navigation
{
    public class AIVisionController : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float _viewRadius = 15f;
        [SerializeField] private float _viewAngle = 90f;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private Transform _visionPoint;
        [SerializeField] private Targeting _targetingComponent;
        
        private Transform _currentTarget;
        private Vector3 _lastKnownPosition;
        private float _timeSinceLastSeen;
        private List<Transform> _visibleTargets = new List<Transform>();

        public Transform CurrentTarget => _currentTarget;
        public Vector3 LastKnownPosition => _lastKnownPosition;
        public bool HasTarget => _currentTarget != null;
        public float TimeSinceLastSeen => _timeSinceLastSeen;
        public float DistanceToTarget => HasTarget ? Vector3.Distance(transform.position, _currentTarget.position) : float.MaxValue;

        public Action<Transform> OnTargetDetected;
        public Action OnTargetLost;
        
        private void Update()
        {
            FindVisibleTargets();
            
            if (HasTarget)
            {
                if (CanSeeTarget(_currentTarget))
                {
                    _lastKnownPosition = _currentTarget.position;
                    _timeSinceLastSeen = 0f;
                }
                else
                {
                    _timeSinceLastSeen += Time.deltaTime;
                    
                    if (_timeSinceLastSeen > 5f)
                    {
                        ClearTarget();
                    }
                }
            }
        }

        private void FindVisibleTargets()
        {
            if (_targetingComponent == null)
                return;
                
            _visibleTargets.Clear();
            Collider[] targetsInRadius = Physics.OverlapSphere(_visionPoint.position, _viewRadius, _targetMask);
            
            foreach (Collider target in targetsInRadius)
            {
                Transform targetTransform = target.transform;
                Targeting targetFaction = targetTransform.GetComponent<Targeting>();
                
                if (targetFaction == null) 
                    continue;
                    
                if (!_targetingComponent.IsHostileTowards(targetFaction))
                    continue;
                
                if (CanSeeTarget(targetTransform))
                {
                    _visibleTargets.Add(targetTransform);
                }
            }
            
            Transform closestTarget = GetClosestTarget();
            
            if (closestTarget != null)
            {
                if (_currentTarget == null)
                {
                    SetTarget(closestTarget);
                }
                else if (_currentTarget != closestTarget)
                {
                    SetTarget(closestTarget);
                }
            }
        }
        
        private Transform GetClosestTarget()
        {
            if (_visibleTargets.Count == 0)
                return null;
                
            Transform closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (Transform target in _visibleTargets)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = target;
                }
            }
            
            return closest;
        }

        private bool CanSeeTarget(Transform target)
        {
            if (target == null)
                return false;
                
            Vector3 directionToTarget = (target.position - _visionPoint.position).normalized;
            float distanceToTarget = Vector3.Distance(_visionPoint.position, target.position);
            
            if (Vector3.Angle(_visionPoint.forward, directionToTarget) > _viewAngle / 2)
                return false;
                
            if (Physics.Raycast(_visionPoint.position, directionToTarget, distanceToTarget, _obstacleMask))
                return false;
                
            return true;
        }
        
        private void SetTarget(Transform target)
        {
            _currentTarget = target;
            _lastKnownPosition = target.position;
            _timeSinceLastSeen = 0f;
            OnTargetDetected?.Invoke(target);
        }
        
        private void ClearTarget()
        {
            _currentTarget = null;
            OnTargetLost?.Invoke();
        }
        
        public bool IsTargetInAttackRange(float attackRange)
        {
            return HasTarget && DistanceToTarget <= attackRange;
        }
        
        public bool IsTargetInAggressionRange(float aggressionRange)
        {
            return HasTarget && DistanceToTarget <= aggressionRange;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_visionPoint.position, _viewRadius);
            
            Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle/2, 0) * _visionPoint.forward * _viewRadius;
            Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle/2, 0) * _visionPoint.forward * _viewRadius;
            
            Gizmos.DrawLine(_visionPoint.position, _visionPoint.position + leftBoundary);
            Gizmos.DrawLine(_visionPoint.position, _visionPoint.position + rightBoundary);
            
            if (_currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_visionPoint.position, _currentTarget.position);
            }
            
            if (_lastKnownPosition != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(_lastKnownPosition, 1f);
            }
        }
        #endif
    }
}