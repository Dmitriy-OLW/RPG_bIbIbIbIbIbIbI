using UnityEngine;

namespace Enemy.Navigation
{
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AINavigationController _navigation;
        [SerializeField] private AIInputMapper _inputMapper;
        [SerializeField] private Transform[] _patrolPoints;
        
        [Header("Settings")]
        [SerializeField] private float _waitTimeAtPoint = 2f;
        
        private int _currentPatrolIndex;
        private float _waitTimer;
        private bool _isWaiting;

        private void Start()
        {
            if (_patrolPoints.Length > 0)
            {
                SetNextPatrolPoint();
            }
        }

        private void Update()
        {
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    SetNextPatrolPoint();
                }
                return;
            }

            if (_navigation.HasReachedDestination)
            {
                _isWaiting = true;
                _waitTimer = _waitTimeAtPoint;
            }
        }

        private void SetNextPatrolPoint()
        {
            if (_patrolPoints.Length == 0)
                return;

            Vector3 targetPoint = _patrolPoints[_currentPatrolIndex].position;
            _navigation.SetDestination(targetPoint);
            
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }

        private void OnDrawGizmosSelected()
        {
            if (_patrolPoints == null || _patrolPoints.Length == 0)
                return;

            Gizmos.color = Color.green;
            
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                if (_patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(_patrolPoints[i].position, 0.5f);
                    
                    int nextIndex = (i + 1) % _patrolPoints.Length;
                    if (_patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
}