using CharacterCamera;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class PlayerTargeting
    {
        private PlayerHandler _handler;
        
        private readonly List<GameObject> _currentTargetCandidates = new List<GameObject>();
        private GameObject _currentLockOnTarget;
        private Transform _targetLockOnPos;
        private bool _isLockedOn;
        private bool _isAiming;

        public bool IsLockedOn => _isLockedOn;
        public bool IsAiming => _isAiming;
        public GameObject CurrentLockOnTarget => _currentLockOnTarget;
        public Transform TargetLockOnPos => _targetLockOnPos;

        public PlayerTargeting(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void Initialize()
        {
            _targetLockOnPos = _handler.transform.Find("TargetLockOnPos");
        }

        public void SetIsAiming(bool value)
        {
            _isAiming = value;
            _handler.PlayerRotation.SetIsStrafing(!_handler.PlayerRotation.IsSprinting);
        }

        public void AddTargetCandidate(GameObject newTarget)
        {
            if (newTarget != null)
            {
                _currentTargetCandidates.Add(newTarget);
            }
        }

        public void RemoveTarget(GameObject targetToRemove)
        {
            if (_currentTargetCandidates.Contains(targetToRemove))
            {
                _currentTargetCandidates.Remove(targetToRemove);
            }
        }

        public void ToggleLockOn()
        {
            EnableLockOn(!_isLockedOn);
        }

        public void EnableLockOn(bool enable)
        {
            _isLockedOn = enable;
            _handler.PlayerRotation.UpdateStrafingState();
            _handler.CameraController.LockOn(enable, _targetLockOnPos);

            if (enable && _currentLockOnTarget != null)
            {
                _currentLockOnTarget.GetComponent<SampleObjectLockOn>().Highlight(true, true);
            }
        }

        public void UpdateBestTarget()
        {
            GameObject newBestTarget;

            if (_currentTargetCandidates.Count == 0)
            {
                newBestTarget = null;
            }
            else if (_currentTargetCandidates.Count == 1)
            {
                newBestTarget = _currentTargetCandidates[0];
            }
            else
            {
                newBestTarget = null;
                float bestTargetScore = 0f;

                foreach (GameObject target in _currentTargetCandidates)
                {
                    target.GetComponent<SampleObjectLockOn>().Highlight(false, false);

                    float distance = Vector3.Distance(_handler.transform.position, target.transform.position);
                    float distanceScore = 1 / distance * 100;

                    Vector3 targetDirection = target.transform.position - _handler.CameraController.GetCameraPosition();
                    float angleInView = Vector3.Dot(targetDirection.normalized, _handler.CameraController.GetCameraForward());
                    float angleScore = angleInView * 40;

                    float totalScore = distanceScore + angleScore;

                    if (totalScore > bestTargetScore)
                    {
                        bestTargetScore = totalScore;
                        newBestTarget = target;
                    }
                }
            }

            if (!_isLockedOn)
            {
                _currentLockOnTarget = newBestTarget;

                if (_currentLockOnTarget != null)
                {
                    _currentLockOnTarget.GetComponent<SampleObjectLockOn>().Highlight(true, false);
                }
            }
            else
            {
                if (_currentTargetCandidates.Contains(_currentLockOnTarget))
                {
                    _currentLockOnTarget.GetComponent<SampleObjectLockOn>().Highlight(true, true);
                    
                    if (_targetLockOnPos != null && _currentLockOnTarget != null)
                    {
                        _targetLockOnPos.position = _currentLockOnTarget.transform.position;
                    }
                }
                else
                {
                    _currentLockOnTarget = newBestTarget;
                    EnableLockOn(false);
                }
            }
        }
    }
}