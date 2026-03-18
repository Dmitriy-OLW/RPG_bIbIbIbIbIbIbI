using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Enemy.Navigation
{
    public class AINavigationController : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] private float _waypointReachedDistance = 0.5f;
        [SerializeField] private float _pathUpdateRate = 0.25f;
        [SerializeField] private bool _drawDebugPath = true;
        [SerializeField] private NavMeshAgent _agent;
        
        private List<Vector3> _currentPath = new List<Vector3>();
        private int _currentWaypointIndex;
        private float _pathUpdateTimer;
        private Vector3 _currentDestination;

        public bool HasPath => _currentPath != null && _currentPath.Count > 0;
        public bool HasReachedDestination => _currentWaypointIndex >= _currentPath.Count - 1;
        public Vector3 CurrentWaypoint 
        { 
            get 
            {
                if(_currentWaypointIndex >= _currentPath.Count || !HasPath)
                    return Vector3.zero;

                return _currentPath[_currentWaypointIndex];
            }
        }

        private void Awake()
        {
            _agent.updatePosition = true;
            _agent.updateRotation = false; 
        }

        private void Update()
        {
            UpdatePathTimer();
        }
        
        public void Stop()
        {
            _agent.isStopped = true;
            ClearPath();
        }

        public void Resume()
        {
            _agent.isStopped = false;
        }
        
        public void Warp(Vector3 position)
        {
            _agent.Warp(position);
        }

        public void SetDestination(Vector3 destination)
        {
            if (Vector3.Distance(_currentDestination, destination) < 0.1f && HasPath)
                return;

            _currentDestination = destination;
            BuildPathToDestination();
        }

        public void UpdateWaypointProgress(Vector3 currentPosition)
        {
            if (!HasPath)
                return;

            float distanceToWaypoint = Vector3.Distance(currentPosition, CurrentWaypoint);
            
            if (distanceToWaypoint <= _waypointReachedDistance)
            {
                _currentWaypointIndex++;
            }
        }

        public void ClearPath()
        {
            _currentPath.Clear();
            _currentWaypointIndex = 0;
            _agent.ResetPath();
        }

        private void UpdatePathTimer()
        {
            _pathUpdateTimer -= Time.deltaTime;
            
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = _pathUpdateRate;
                
                if (_agent.hasPath && _agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    BuildPathToDestination();
                }
            }
        }

        private void BuildPathToDestination()
        {
            NavMeshPath path = new NavMeshPath();
            
            if (_agent.CalculatePath(_currentDestination, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                _currentPath.Clear();
                _currentPath.AddRange(path.corners);
                _currentWaypointIndex = 1; 
                
                if (_drawDebugPath)
                {
                    DebugPath();
                }
            }
        }

        private void DebugPath()
        {
            if (_currentPath.Count < 2)
                return;

            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                Debug.DrawLine(_currentPath[i], _currentPath[i + 1], Color.cyan, _pathUpdateRate);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!_drawDebugPath || _currentPath == null || _currentPath.Count == 0)
                return;

            Gizmos.color = Color.yellow;
            
            for (int i = 0; i < _currentPath.Count; i++)
            {
                Gizmos.DrawSphere(_currentPath[i], 0.3f);
                
                if (i == _currentWaypointIndex)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(_currentPath[i], 0.5f);
                    Gizmos.color = Color.yellow;
                }
            }
        }
    }
}