using UnityEngine;
using Character.InputController;

namespace Enemy.Navigation
{
    public class AIInputMapper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AIInputReader _inputReader;
        [SerializeField] private AINavigationController _navigation;
        [SerializeField] private Transform _enemyTransform;
        
        [Header("Movement Settings")]
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _rotationThreshold = 5f;

        private Vector3 _currentWaypoint;
        private bool _isMoving;
        private bool _shouldRun;

        private void Update()
        {
            if (!_navigation.HasPath)
            {
                StopMoving();
                return;
            }

            _currentWaypoint = _navigation.CurrentWaypoint;
            
            HandleRotation();
            HandleMovement();
            UpdateWaypointProgress();
        }
        
        public void SetShouldRun(bool shouldRun) => _shouldRun = shouldRun;

        private void HandleRotation()
        {
            Vector3 directionToWaypoint = (_currentWaypoint - _enemyTransform.position).normalized;
            
            float angleToWaypoint = Vector3.SignedAngle(_enemyTransform.forward, directionToWaypoint, Vector3.up);
            
            float rotationInput = Mathf.Clamp(angleToWaypoint / 180f, -1f, 1f);
            
            _inputReader.SetLookDirection(new Vector2(rotationInput * _rotationSpeed, 0f));
            
            bool isFacingWaypoint = Mathf.Abs(angleToWaypoint) < _rotationThreshold;
            
            if (isFacingWaypoint)
            {
                _inputReader.SetLookDirection(Vector2.zero);
            }
        }

        private void HandleMovement()
        {
            Vector3 directionToWaypoint = (_currentWaypoint - _enemyTransform.position).normalized;
            
            Vector3 localDirection = _enemyTransform.InverseTransformDirection(directionToWaypoint);
            
            Vector2 moveInput = Vector2.zero;
            
            if (localDirection.z > 0) 
            {
                moveInput.y = Mathf.Clamp01(localDirection.z);
            }
            else if (localDirection.z < 0)
            {
                moveInput.y = -Mathf.Clamp01(-localDirection.z);
            }
            
            moveInput.x = Mathf.Clamp(localDirection.x, -1f, 1f);
            
            _inputReader.SetMoveDirection(moveInput);
            
            _isMoving = moveInput.magnitude > 0.1f;
            
            _inputReader.SetSprint(_shouldRun);
        }

        private void UpdateWaypointProgress()
        {
            _navigation.UpdateWaypointProgress(_enemyTransform.position);
        }

        #region Debug

        private void StopMoving()
        {
            if (_isMoving)
            {
                _inputReader.SetMoveDirection(Vector2.zero);
                _inputReader.SetLookDirection(Vector2.zero);
                _inputReader.SetSprint(false);
                _isMoving = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_navigation.HasPath)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_enemyTransform.position, _currentWaypoint);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_enemyTransform.position, _enemyTransform.forward * 2f);
            }
        }

        #endregion
    }
}