using Character.AnimationController;
using UnityEngine;

namespace Character
{
    public class PlayerMovement
    {
        private PlayerHandler _handler;
        
        private float _currentMaxSpeed;
        private float _targetMaxSpeed;
        private Vector3 _targetVelocity;
        private Vector3 _velocity;
        private Vector3 _moveDirection;
        private float _speed2D;
        private float _fallStartTime;
        private float _fallingDuration;
        private float _locomotionStartTimer;
        private float _locomotionStartDirection;
        private bool _isStarting;
        private bool _isStopped = true;
        private bool _movementInputHeld;
        private bool _movementInputPressed;
        private bool _movementInputTapped;
        private AnimatorController.GaitState _currentGait;

        public Vector3 Velocity => _velocity;
        public float Speed2D => _speed2D;
        public float FallingDuration => _fallingDuration;
        public float LocomotionStartDirection => _locomotionStartDirection;
        public bool IsStarting => _isStarting;
        public bool IsStopped => _isStopped;
        public bool MovementInputHeld => _movementInputHeld;
        public bool MovementInputPressed => _movementInputPressed;
        public bool MovementInputTapped => _movementInputTapped;
        public AnimatorController.GaitState CurrentGait => _currentGait;
        public Vector3 MoveDirection => _moveDirection;

        public PlayerMovement(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void CalculateInput()
        {
            if (_handler.InputReader._movementInputDetected)
            {
                if (_handler.InputReader._movementInputDuration == 0)
                {
                    _movementInputTapped = true;
                }
                else if (_handler.InputReader._movementInputDuration > 0 && 
                         _handler.InputReader._movementInputDuration < _handler.Config.ButtonHoldThreshold)
                {
                    _movementInputTapped = false;
                    _movementInputPressed = true;
                    _movementInputHeld = false;
                }
                else
                {
                    _movementInputTapped = false;
                    _movementInputPressed = false;
                    _movementInputHeld = true;
                }

                _handler.InputReader._movementInputDuration += Time.deltaTime;
            }
            else
            {
                _handler.InputReader._movementInputDuration = 0;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }

            _moveDirection = (_handler.CameraController.GetCameraForwardZeroedYNormalised() * _handler.InputReader._moveComposite.y)
                + (_handler.CameraController.GetCameraRightZeroedYNormalised() * _handler.InputReader._moveComposite.x);
        }

        public void CalculateMoveDirection()
        {
            CalculateInput();

            if (!_handler.PlayerGroundedChecker.IsGrounded)
            {
                _targetMaxSpeed = _currentMaxSpeed;
            }
            else if (_handler.PlayerCrouch.IsCrouching)
            {
                _targetMaxSpeed = _handler.Config.WalkSpeed;
            }
            else if (_handler.PlayerRotation.IsSprinting)
            {
                _targetMaxSpeed = _handler.Config.SprintSpeed;
            }
            else if (_handler.PlayerRotation.IsWalking)
            {
                _targetMaxSpeed = _handler.Config.WalkSpeed;
            }
            else
            {
                _targetMaxSpeed = _handler.Config.RunSpeed;
            }

            _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, _handler.Config.AnimationDampTime * Time.deltaTime);

            _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

            _velocity.z = Mathf.Lerp(_velocity.z, _targetVelocity.z, _handler.Config.SpeedChangeDamping * Time.deltaTime);
            _velocity.x = Mathf.Lerp(_velocity.x, _targetVelocity.x, _handler.Config.SpeedChangeDamping * Time.deltaTime);

            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

            Vector3 playerForwardVector = _handler.transform.forward;
            float newDirectionDifferenceAngle = playerForwardVector != _moveDirection
                ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
                : 0f;

            CalculateGait();
        }

        private void CalculateGait()
        {
            float runThreshold = (_handler.Config.WalkSpeed + _handler.Config.RunSpeed) / 2;
            float sprintThreshold = (_handler.Config.RunSpeed + _handler.Config.SprintSpeed) / 2;

            if (_speed2D < 0.01)
            {
                _currentGait = AnimatorController.GaitState.Idle;
            }
            else if (_speed2D < runThreshold)
            {
                _currentGait = AnimatorController.GaitState.Walk;
            }
            else if (_speed2D < sprintThreshold)
            {
                _currentGait = AnimatorController.GaitState.Run;
            }
            else
            {
                _currentGait = AnimatorController.GaitState.Sprint;
            }
        }

        public void Move()
        {
            _handler.Controller.Move(_velocity * Time.deltaTime);
        }

        public void ApplyGravity()
        {
            if (_velocity.y > Physics.gravity.y)
            {
                _velocity.y += Physics.gravity.y * _handler.Config.GravityMultiplier * Time.deltaTime;
            }
        }

        public void SetVelocityY(float value)
        {
            _velocity.y = value;
        }

        public void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            _fallingDuration = 0f;
        }

        public void UpdateFallingDuration()
        {
            _fallingDuration = Time.time - _fallStartTime;
        }

        public void CheckIfStopped()
        {
            _isStopped = _moveDirection.magnitude == 0 && _speed2D < .5;
        }

        public void CheckIfStarting()
        {
            _locomotionStartTimer = VariableOverrideDelayTimer(_locomotionStartTimer);

            bool isStartingCheck = false;

            if (_locomotionStartTimer <= 0.0f)
            {
                if (_moveDirection.magnitude > 0.01 && _speed2D < 1 && !_handler.PlayerRotation.IsStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        Vector3 playerForwardVector = _handler.transform.forward;
                        _locomotionStartDirection = playerForwardVector != _moveDirection
                            ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
                            : 0f;
                        _handler.AnimatorController.SetLocomotionStartDirection(_locomotionStartDirection);
                    }

                    float delayTime = 0.2f;
                    _handler.PlayerLocomotionAdditives.SetDelays(delayTime);
                    _locomotionStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
        }

        private float VariableOverrideDelayTimer(float timeVariable)
        {
            if (timeVariable > 0.0f)
            {
                timeVariable -= Time.deltaTime;
                timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
            }
            else
            {
                timeVariable = 0.0f;
            }

            return timeVariable;
        }

        public void UpdateAnimatorController()
        {
            var animatorData = new AnimatorController.AnimatorData
            {
                LeanValue = _handler.PlayerLocomotionAdditives.LeanValue,
                HeadLookX = _handler.PlayerLocomotionAdditives.HeadLookX,
                HeadLookY = _handler.PlayerLocomotionAdditives.HeadLookY,
                BodyLookX = _handler.PlayerLocomotionAdditives.BodyLookX,
                BodyLookY = _handler.PlayerLocomotionAdditives.BodyLookY,
                IsStrafing = _handler.PlayerRotation.IsStrafing,
                InclineAngle = _handler.PlayerGroundedChecker.InclineAngle,
                Speed2D = _speed2D,
                CurrentGait = _currentGait,
                StrafeDirectionX = _handler.PlayerRotation.StrafeDirectionX,
                StrafeDirectionZ = _handler.PlayerRotation.StrafeDirectionZ,
                ForwardStrafe = _handler.PlayerRotation.ForwardStrafe,
                CameraRotationOffset = _handler.Config.CameraRotationOffset,
                MovementInputHeld = _movementInputHeld,
                MovementInputPressed = _movementInputPressed,
                MovementInputTapped = _movementInputTapped,
                ShuffleDirectionX = _handler.PlayerRotation.ShuffleDirectionX,
                ShuffleDirectionZ = _handler.PlayerRotation.ShuffleDirectionZ,
                IsTurningInPlace = _handler.PlayerRotation.IsTurningInPlace,
                IsCrouching = _handler.PlayerCrouch.IsCrouching,
                FallingDuration = _fallingDuration,
                IsGrounded = _handler.PlayerGroundedChecker.IsGrounded,
                IsWalking = _handler.PlayerRotation.IsWalking,
                IsStopped = _isStopped,
                LocomotionStartDirection = _locomotionStartDirection
            };

            _handler.AnimatorController.UpdateAnimatorParameters(animatorData);
        }
    }
}