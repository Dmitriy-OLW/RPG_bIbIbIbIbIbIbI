using UnityEngine;

namespace Character
{
    public class PlayerRotation
    {
        private PlayerHandler _handler;
        
        private float _strafeAngle;
        private float _strafeDirectionX;
        private float _strafeDirectionZ;
        private float _forwardStrafe = 1f;
        private float _shuffleDirectionX;
        private float _shuffleDirectionZ;
        private bool _isTurningInPlace;
        private bool _isStrafing;
        private bool _isWalking;
        private bool _isSprinting;
        private Vector3 _cameraForward;
        private Vector3 _previousRotation;
        private Vector3 _currentRotation = new Vector3(0f, 0f, 0f);

        public float StrafeDirectionX => _strafeDirectionX;
        public float StrafeDirectionZ => _strafeDirectionZ;
        public float ForwardStrafe => _forwardStrafe;
        public float ShuffleDirectionX => _shuffleDirectionX;
        public float ShuffleDirectionZ => _shuffleDirectionZ;
        public bool IsTurningInPlace => _isTurningInPlace;
        public bool IsStrafing => _isStrafing;
        public bool IsWalking => _isWalking;
        public bool IsSprinting => _isSprinting;

        public PlayerRotation(PlayerHandler handler)
        {
            _handler = handler;
            _isStrafing = _handler.Config.AlwaysStrafe;
        }

        public void SetIsWalking(bool value)
        {
            _isWalking = value && _handler.PlayerGroundedChecker.IsGrounded && !_isSprinting;
        }

        public void SetIsSprinting(bool value)
        {
            _isSprinting = value;
        }

        public void SetIsStrafing(bool value)
        {
            _isStrafing = value;
        }

        public void UpdateStrafingState()
        {
            _isStrafing = _handler.PlayerTargeting.IsLockedOn 
                ? !_isSprinting 
                : _handler.Config.AlwaysStrafe || _handler.PlayerTargeting.IsAiming;
        }

        public void FaceMoveDirection()
        {
            Vector3 characterForward = new Vector3(_handler.transform.forward.x, 0f, _handler.transform.forward.z).normalized;
            Vector3 characterRight = new Vector3(_handler.transform.right.x, 0f, _handler.transform.right.z).normalized;
            Vector3 directionForward = new Vector3(_handler.PlayerMovement.MoveDirection.x, 0f, _handler.PlayerMovement.MoveDirection.z).normalized;

            _cameraForward = _handler.CameraController.GetCameraForwardZeroedYNormalised();
            Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

            _strafeAngle = characterForward != directionForward 
                ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) 
                : 0f;

            _isTurningInPlace = false;

            if (_isStrafing)
            {
                if (_handler.PlayerMovement.MoveDirection.magnitude > 0.01)
                {
                    if (_cameraForward != Vector3.zero)
                    {
                        _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                        _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                        UpdateStrafeDirection(
                            Vector3.Dot(characterForward, directionForward),
                            Vector3.Dot(characterRight, directionForward)
                        );
                        _handler.Config.CameraRotationOffset = Mathf.Lerp(_handler.Config.CameraRotationOffset, 0f, 
                            _handler.Config.RotationSmoothing * Time.deltaTime);

                        float targetValue = _strafeAngle > _handler.Config.ForwardStrafeMinThreshold && 
                                          _strafeAngle < _handler.Config.ForwardStrafeMaxThreshold ? 1f : 0f;

                        if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                        {
                            _forwardStrafe = targetValue;
                        }
                        else
                        {
                            float t = Mathf.Clamp01(_handler.Config.StrafeDirectionDampTime * Time.deltaTime);
                            _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                        }
                    }

                    _handler.transform.rotation = Quaternion.Slerp(_handler.transform.rotation, strafingTargetRotation, 
                        _handler.Config.RotationSmoothing * Time.deltaTime);
                }
                else
                {
                    UpdateStrafeDirection(1f, 0f);

                    float t = 20 * Time.deltaTime;
                    float newOffset = 0f;

                    if (characterForward != _cameraForward)
                    {
                        newOffset = Vector3.SignedAngle(characterForward, _cameraForward, Vector3.up);
                    }

                    _handler.Config.CameraRotationOffset = Mathf.Lerp(_handler.Config.CameraRotationOffset, newOffset, t);

                    if (Mathf.Abs(_handler.Config.CameraRotationOffset) > 10)
                    {
                        _isTurningInPlace = true;
                    }
                }
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);
                _handler.Config.CameraRotationOffset = Mathf.Lerp(_handler.Config.CameraRotationOffset, 0f, 
                    _handler.Config.RotationSmoothing * Time.deltaTime);

                _shuffleDirectionZ = 1;
                _shuffleDirectionX = 0;

                Vector3 faceDirection = new Vector3(_handler.PlayerMovement.Velocity.x, 0f, _handler.PlayerMovement.Velocity.z);

                if (faceDirection == Vector3.zero)
                {
                    return;
                }

                _handler.transform.rotation = Quaternion.Slerp(
                    _handler.transform.rotation,
                    Quaternion.LookRotation(faceDirection),
                    _handler.Config.RotationSmoothing * Time.deltaTime
                );
            }
        }

        private void UpdateStrafeDirection(float TargetZ, float TargetX)
        {
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, TargetZ, _handler.Config.AnimationDampTime * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, TargetX, _handler.Config.AnimationDampTime * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
        }

        public void StorePreviousRotation()
        {
            _previousRotation = _handler.transform.forward;
        }

        public void UpdateCurrentRotation()
        {
            _currentRotation = _handler.transform.forward;
        }

        public float GetRotationRate()
        {
            return _currentRotation != _previousRotation
                ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                : 0f;
        }
    }
}