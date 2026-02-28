using Character.AnimationController;
using Character.InputController;
using CharacterCamera;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class SamplePlayerAnimationController : MonoBehaviour
    {
        private enum AnimationState
        {
            Base,
            Locomotion,
            Jump,
            Fall,
            Crouch
        }

        [Header("External Components")]
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private AnimatorController _animatorController;
        [SerializeField] private PlayerConfig _config;

        [Header("Shuffle Settings")]
        [SerializeField] private float _shuffleDirectionX;
        [SerializeField] private float _shuffleDirectionZ;

        [Header("Player Strafing")]
        [SerializeField] private float _forwardStrafe = 1f;

        [Header("Grounded Angle")]
        [SerializeField] private Transform _rearRayPos;
        [SerializeField] private Transform _frontRayPos;
        [SerializeField] private float _inclineAngle;

        [Header("In-Air Settings")]
        [SerializeField] private float _fallingDuration;

        [Header("Head Look Settings")]
        [SerializeField] private bool _enableHeadTurn = true;
        [SerializeField] private float _headLookDelay;
        [SerializeField] private float _headLookX;
        [SerializeField] private float _headLookY;

        [Header("Body Look Settings")]
        [SerializeField] private bool _enableBodyTurn = true;
        [SerializeField] private float _bodyLookDelay;
        [SerializeField] private float _bodyLookX;
        [SerializeField] private float _bodyLookY;

        [Header("Lean Settings")]
        [SerializeField] private bool _enableLean = true;
        [SerializeField] private float _leanDelay;
        [SerializeField] private float _leanValue;
        [SerializeField] private bool _animationClipEnd;

        private readonly List<GameObject> _currentTargetCandidates = new List<GameObject>();
        private AnimationState _currentState = AnimationState.Base;
        private bool _cannotStandUp;
        private bool _crouchKeyPressed;
        private bool _isAiming;
        private bool _isCrouching;
        private bool _isGrounded = true;
        private bool _isLockedOn;
        private bool _isSliding;
        private bool _isSprinting;
        private bool _isStarting;
        private bool _isStopped = true;
        private bool _isStrafing;
        private bool _isTurningInPlace;
        private bool _isWalking;
        private bool _movementInputHeld;
        private bool _movementInputPressed;
        private bool _movementInputTapped;
        private float _currentMaxSpeed;
        private float _locomotionStartDirection;
        private float _locomotionStartTimer;
        private float _lookingAngle;
        private float _newDirectionDifferenceAngle;
        private float _speed2D;
        private float _strafeAngle;
        private float _strafeDirectionX;
        private float _strafeDirectionZ;
        private GameObject _currentLockOnTarget;
        private AnimatorController.GaitState _currentGait;
        private Transform _targetLockOnPos;
        private Vector3 _currentRotation = new Vector3(0f, 0f, 0f);
        private Vector3 _moveDirection;
        private Vector3 _previousRotation;
        private Vector3 _velocity;

        private float _targetMaxSpeed;
        private float _fallStartTime;
        private float _rotationRate;
        private float _initialLeanValue;
        private float _initialTurnValue;
        private Vector3 _cameraForward;
        private Vector3 _targetVelocity;

        private void Start()
        {
            _targetLockOnPos = transform.Find("TargetLockOnPos");

            _inputReader.onLockOnToggled += ToggleLockOn;
            _inputReader.onWalkToggled += ToggleWalk;
            _inputReader.onSprintActivated += ActivateSprint;
            _inputReader.onSprintDeactivated += DeactivateSprint;
            _inputReader.onCrouchActivated += ActivateCrouch;
            _inputReader.onCrouchDeactivated += DeactivateCrouch;
            _inputReader.onAimActivated += ActivateAim;
            _inputReader.onAimDeactivated += DeactivateAim;

            _isStrafing = _config.AlwaysStrafe;

            SwitchState(AnimationState.Locomotion);
        }

        private void ActivateAim()
        {
            _isAiming = true;
            _isStrafing = !_isSprinting;
        }

        private void DeactivateAim()
        {
            _isAiming = false;
            _isStrafing = !_isSprinting && (_config.AlwaysStrafe || _isLockedOn);
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

        private void ToggleLockOn()
        {
            EnableLockOn(!_isLockedOn);
        }

        private void EnableLockOn(bool enable)
        {
            _isLockedOn = enable;
            _isStrafing = false;

            _isStrafing = enable ? !_isSprinting : _config.AlwaysStrafe || _isAiming;

            _cameraController.LockOn(enable, _targetLockOnPos);

            if (enable && _currentLockOnTarget != null)
            {
                _currentLockOnTarget.GetComponent<SampleObjectLockOn>().Highlight(true, true);
            }
        }

        private void ToggleWalk()
        {
            EnableWalk(!_isWalking);
        }

        private void EnableWalk(bool enable)
        {
            _isWalking = enable && _isGrounded && !_isSprinting;
        }

        private void ActivateSprint()
        {
            if (!_isCrouching)
            {
                EnableWalk(false);
                _isSprinting = true;
                _isStrafing = false;
            }
        }

        private void DeactivateSprint()
        {
            _isSprinting = false;

            if (_config.AlwaysStrafe || _isAiming || _isLockedOn)
            {
                _isStrafing = true;
            }
        }

        private void ActivateCrouch()
        {
            _crouchKeyPressed = true;

            if (_isGrounded)
            {
                CapsuleCrouchingSize(true);
                DeactivateSprint();
                _isCrouching = true;
            }
        }

        private void DeactivateCrouch()
        {
            _crouchKeyPressed = false;

            if (!_cannotStandUp && !_isSliding)
            {
                CapsuleCrouchingSize(false);
                _isCrouching = false;
            }
        }

        public void ActivateSliding()
        {
            _isSliding = true;
        }

        public void DeactivateSliding()
        {
            _isSliding = false;
        }

        private void CapsuleCrouchingSize(bool crouching)
        {
            if (crouching)
            {
                _controller.center = new Vector3(0f, _config.CapsuleCrouchingCentre, 0f);
                _controller.height = _config.CapsuleCrouchingHeight;
            }
            else
            {
                _controller.center = new Vector3(0f, _config.CapsuleStandingCentre, 0f);
                _controller.height = _config.CapsuleStandingHeight;
            }
        }

        private void SwitchState(AnimationState newState)
        {
            ExitCurrentState();
            EnterState(newState);
        }

        private void EnterState(AnimationState stateToEnter)
        {
            _currentState = stateToEnter;
            switch (_currentState)
            {
                case AnimationState.Base:
                    EnterBaseState();
                    break;
                case AnimationState.Locomotion:
                    EnterLocomotionState();
                    break;
                case AnimationState.Jump:
                    EnterJumpState();
                    break;
                case AnimationState.Fall:
                    EnterFallState();
                    break;
                case AnimationState.Crouch:
                    EnterCrouchState();
                    break;
            }
        }

        private void ExitCurrentState()
        {
            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    ExitLocomotionState();
                    break;
                case AnimationState.Jump:
                    ExitJumpState();
                    break;
                case AnimationState.Crouch:
                    ExitCrouchState();
                    break;
            }
        }

        private void Update()
        {
            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    UpdateLocomotionState();
                    break;
                case AnimationState.Jump:
                    UpdateJumpState();
                    break;
                case AnimationState.Fall:
                    UpdateFallState();
                    break;
                case AnimationState.Crouch:
                    UpdateCrouchState();
                    break;
            }
        }

        private void UpdateAnimatorController()
        {
            var animatorData = new AnimatorController.AnimatorData
            {
                LeanValue = _leanValue,
                HeadLookX = _headLookX,
                HeadLookY = _headLookY,
                BodyLookX = _bodyLookX,
                BodyLookY = _bodyLookY,
                IsStrafing = _isStrafing,
                InclineAngle = _inclineAngle,
                Speed2D = _speed2D,
                CurrentGait = _currentGait,
                StrafeDirectionX = _strafeDirectionX,
                StrafeDirectionZ = _strafeDirectionZ,
                ForwardStrafe = _forwardStrafe,
                CameraRotationOffset = _config.CameraRotationOffset,
                MovementInputHeld = _movementInputHeld,
                MovementInputPressed = _movementInputPressed,
                MovementInputTapped = _movementInputTapped,
                ShuffleDirectionX = _shuffleDirectionX,
                ShuffleDirectionZ = _shuffleDirectionZ,
                IsTurningInPlace = _isTurningInPlace,
                IsCrouching = _isCrouching,
                FallingDuration = _fallingDuration,
                IsGrounded = _isGrounded,
                IsWalking = _isWalking,
                IsStopped = _isStopped,
                LocomotionStartDirection = _locomotionStartDirection
            };

            _animatorController.UpdateAnimatorParameters(animatorData);
        }

        private void EnterBaseState()
        {
            _previousRotation = transform.forward;
        }

        private void CalculateInput()
        {
            if (_inputReader._movementInputDetected)
            {
                if (_inputReader._movementInputDuration == 0)
                {
                    _movementInputTapped = true;
                }
                else if (_inputReader._movementInputDuration > 0 && _inputReader._movementInputDuration < _config.ButtonHoldThreshold)
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

                _inputReader._movementInputDuration += Time.deltaTime;
            }
            else
            {
                _inputReader._movementInputDuration = 0;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }

            _moveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * _inputReader._moveComposite.y)
                + (_cameraController.GetCameraRightZeroedYNormalised() * _inputReader._moveComposite.x);
        }

        private void Move()
        {
            _controller.Move(_velocity * Time.deltaTime);

            if (_isLockedOn)
            {
                if (_currentLockOnTarget != null)
                {
                    _targetLockOnPos.position = _currentLockOnTarget.transform.position;
                }
            }
        }

        private void ApplyGravity()
        {
            if (_velocity.y > Physics.gravity.y)
            {
                _velocity.y += Physics.gravity.y * _config.GravityMultiplier * Time.deltaTime;
            }
        }

        private void CalculateMoveDirection()
        {
            CalculateInput();

            if (!_isGrounded)
            {
                _targetMaxSpeed = _currentMaxSpeed;
            }
            else if (_isCrouching)
            {
                _targetMaxSpeed = _config.WalkSpeed;
            }
            else if (_isSprinting)
            {
                _targetMaxSpeed = _config.SprintSpeed;
            }
            else if (_isWalking)
            {
                _targetMaxSpeed = _config.WalkSpeed;
            }
            else
            {
                _targetMaxSpeed = _config.RunSpeed;
            }

            _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, _config.AnimationDampTime * Time.deltaTime);

            _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

            _velocity.z = Mathf.Lerp(_velocity.z, _targetVelocity.z, _config.SpeedChangeDamping * Time.deltaTime);
            _velocity.x = Mathf.Lerp(_velocity.x, _targetVelocity.x, _config.SpeedChangeDamping * Time.deltaTime);

            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

            Vector3 playerForwardVector = transform.forward;

            _newDirectionDifferenceAngle = playerForwardVector != _moveDirection
                ? Vector3.SignedAngle(playerForwardVector, _moveDirection, Vector3.up)
                : 0f;

            CalculateGait();
        }

        private void CalculateGait()
        {
            float runThreshold = (_config.WalkSpeed + _config.RunSpeed) / 2;
            float sprintThreshold = (_config.RunSpeed + _config.SprintSpeed) / 2;

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

        private void FaceMoveDirection()
        {
            Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 directionForward = new Vector3(_moveDirection.x, 0f, _moveDirection.z).normalized;

            _cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
            Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

            _strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;

            _isTurningInPlace = false;

            if (_isStrafing)
            {
                if (_moveDirection.magnitude > 0.01)
                {
                    if (_cameraForward != Vector3.zero)
                    {
                        _shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                        _shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                        UpdateStrafeDirection(
                            Vector3.Dot(characterForward, directionForward),
                            Vector3.Dot(characterRight, directionForward)
                        );
                        _config.CameraRotationOffset = Mathf.Lerp(_config.CameraRotationOffset, 0f, _config.RotationSmoothing * Time.deltaTime);

                        float targetValue = _strafeAngle > _config.ForwardStrafeMinThreshold && _strafeAngle < _config.ForwardStrafeMaxThreshold ? 1f : 0f;

                        if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                        {
                            _forwardStrafe = targetValue;
                        }
                        else
                        {
                            float t = Mathf.Clamp01(_config.StrafeDirectionDampTime * Time.deltaTime);
                            _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                        }
                    }

                    transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _config.RotationSmoothing * Time.deltaTime);
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

                    _config.CameraRotationOffset = Mathf.Lerp(_config.CameraRotationOffset, newOffset, t);

                    if (Mathf.Abs(_config.CameraRotationOffset) > 10)
                    {
                        _isTurningInPlace = true;
                    }
                }
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);
                _config.CameraRotationOffset = Mathf.Lerp(_config.CameraRotationOffset, 0f, _config.RotationSmoothing * Time.deltaTime);

                _shuffleDirectionZ = 1;
                _shuffleDirectionX = 0;

                Vector3 faceDirection = new Vector3(_velocity.x, 0f, _velocity.z);

                if (faceDirection == Vector3.zero)
                {
                    return;
                }

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(faceDirection),
                    _config.RotationSmoothing * Time.deltaTime
                );
            }
        }

        private void CheckIfStopped()
        {
            _isStopped = _moveDirection.magnitude == 0 && _speed2D < .5;
        }

        private void CheckIfStarting()
        {
            _locomotionStartTimer = VariableOverrideDelayTimer(_locomotionStartTimer);

            bool isStartingCheck = false;

            if (_locomotionStartTimer <= 0.0f)
            {
                if (_moveDirection.magnitude > 0.01 && _speed2D < 1 && !_isStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        _locomotionStartDirection = _newDirectionDifferenceAngle;
                        _animatorController.SetLocomotionStartDirection(_locomotionStartDirection);
                    }

                    float delayTime = 0.2f;
                    _leanDelay = delayTime;
                    _headLookDelay = delayTime;
                    _bodyLookDelay = delayTime;

                    _locomotionStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
        }

        private void UpdateStrafeDirection(float TargetZ, float TargetX)
        {
            _strafeDirectionZ = Mathf.Lerp(_strafeDirectionZ, TargetZ, _config.AnimationDampTime * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(_strafeDirectionX, TargetX, _config.AnimationDampTime * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(_strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(_strafeDirectionX * 1000f) / 1000f;
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _controller.transform.position.x,
                _controller.transform.position.y - _config.GroundedOffset,
                _controller.transform.position.z
            );
            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _config.GroundLayerMask, QueryTriggerInteraction.Ignore);

            if (_isGrounded)
            {
                GroundInclineCheck();
            }
        }

        private void GroundInclineCheck()
        {
            float rayDistance = Mathf.Infinity;
            _rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
            _frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);

            Physics.Raycast(_rearRayPos.position, _rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, _config.GroundLayerMask);
            Physics.Raycast(
                _frontRayPos.position,
                _frontRayPos.TransformDirection(-Vector3.up),
                out RaycastHit frontHit,
                rayDistance,
                _config.GroundLayerMask
            );

            Vector3 hitDifference = frontHit.point - rearHit.point;
            float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

            _inclineAngle = Mathf.Lerp(_inclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
        }

        private void CeilingHeightCheck()
        {
            float rayDistance = Mathf.Infinity;
            float minimumStandingHeight = _config.CapsuleStandingHeight - _frontRayPos.localPosition.y;

            Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + _frontRayPos.localPosition.y, transform.position.z);
            if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, _config.GroundLayerMask))
            {
                _cannotStandUp = ceilingHit.distance < minimumStandingHeight;
            }
            else
            {
                _cannotStandUp = false;
            }
        }

        private void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            _fallingDuration = 0f;
        }

        private void UpdateFallingDuration()
        {
            _fallingDuration = Time.time - _fallStartTime;
        }

        private void CheckEnableTurns()
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;
            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        private void CheckEnableLean()
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        private void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _currentRotation = transform.forward;

                _rotationRate = _currentRotation != _previousRotation
                    ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                    : 0f;
            }

            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float referenceValue = _speed2D / _config.SprintSpeed;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _config.LeanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;

            if (headLookActivated && _isTurningInPlace)
            {
                _initialTurnValue = _config.CameraRotationOffset;
                _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
            }
            else
            {
                _initialTurnValue = headLookActivated ? _rotationRate : 0f;
                _headLookX = CalculateSmoothedValue(
                    _headLookX,
                    _initialTurnValue,
                    maxLeanRotationRate,
                    headTurnSmoothness,
                    _config.HeadLookXCurve,
                    _headLookX,
                    false
                );
            }

            float bodyTurnSmoothness = 5f;

            _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

            _bodyLookX = CalculateSmoothedValue(
                _bodyLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                bodyTurnSmoothness,
                _config.BodyLookXCurve,
                _bodyLookX,
                false
            );

            float cameraTilt = _cameraController.GetCameraTiltX();
            cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
            cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _previousRotation = _currentRotation;
        }

        private float CalculateSmoothedValue(
            float mainVariable,
            float newValue,
            float maxRateChange,
            float smoothness,
            AnimationCurve referenceCurve,
            float referenceValue,
            bool isMultiplier
        )
        {
            float changeVariable = newValue / maxRateChange;

            changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

            if (isMultiplier)
            {
                float multiplier = referenceCurve.Evaluate(referenceValue);
                changeVariable *= multiplier;
            }
            else
            {
                changeVariable = referenceCurve.Evaluate(changeVariable);
            }

            if (!changeVariable.Equals(mainVariable))
            {
                changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
            }

            return changeVariable;
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

        private void UpdateBestTarget()
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

                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    float distanceScore = 1 / distance * 100;

                    Vector3 targetDirection = target.transform.position - _cameraController.GetCameraPosition();
                    float angleInView = Vector3.Dot(targetDirection.normalized, _cameraController.GetCameraForward());
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
                }
                else
                {
                    _currentLockOnTarget = newBestTarget;
                    EnableLockOn(false);
                }
            }
        }

        private void EnterLocomotionState()
        {
            _inputReader.onJumpPerformed += LocomotionToJumpState;
        }

        private void UpdateLocomotionState()
        {
            UpdateBestTarget();
            GroundedCheck();

            if (!_isGrounded)
            {
                SwitchState(AnimationState.Fall);
            }

            if (_isCrouching)
            {
                SwitchState(AnimationState.Crouch);
            }

            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();
            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        private void ExitLocomotionState()
        {
            _inputReader.onJumpPerformed -= LocomotionToJumpState;
        }

        private void LocomotionToJumpState()
        {
            SwitchState(AnimationState.Jump);
        }

        private void EnterJumpState()
        {
            _animatorController.SetIsJumping(true);

            _isSliding = false;

            _velocity = new Vector3(_velocity.x, _config.JumpForce, _velocity.z);
        }

        private void UpdateJumpState()
        {
            UpdateBestTarget();
            ApplyGravity();

            if (_velocity.y <= 0f)
            {
                _animatorController.SetIsJumping(false);
                SwitchState(AnimationState.Fall);
            }

            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            CalculateMoveDirection();
            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        private void ExitJumpState()
        {
            _animatorController.SetIsJumping(false);
        }

        private void EnterFallState()
        {
            ResetFallingDuration();
            _velocity.y = 0f;

            DeactivateCrouch();
            _isSliding = false;
        }

        private void UpdateFallState()
        {
            UpdateBestTarget();
            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);

            CalculateMoveDirection();
            FaceMoveDirection();

            ApplyGravity();
            Move();
            UpdateAnimatorController();

            if (_controller.isGrounded)
            {
                SwitchState(AnimationState.Locomotion);
            }

            UpdateFallingDuration();
        }

        private void EnterCrouchState()
        {
            _inputReader.onJumpPerformed += CrouchToJumpState;
        }

        private void UpdateCrouchState()
        {
            UpdateBestTarget();

            GroundedCheck();
            if (!_isGrounded)
            {
                DeactivateCrouch();
                CapsuleCrouchingSize(false);
                SwitchState(AnimationState.Fall);
            }

            CeilingHeightCheck();

            if (!_crouchKeyPressed && !_cannotStandUp)
            {
                DeactivateCrouch();
                SwitchToLocomotionState();
            }

            if (!_isCrouching)
            {
                CapsuleCrouchingSize(false);
                SwitchToLocomotionState();
            }

            CheckEnableTurns();
            CheckEnableLean();

            CalculateRotationalAdditives(false, _enableHeadTurn, false);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();

            FaceMoveDirection();
            Move();
            UpdateAnimatorController();
        }

        private void ExitCrouchState()
        {
            _inputReader.onJumpPerformed -= CrouchToJumpState;
        }

        private void CrouchToJumpState()
        {
            if (!_cannotStandUp)
            {
                DeactivateCrouch();
                SwitchState(AnimationState.Jump);
            }
        }

        private void SwitchToLocomotionState()
        {
            DeactivateCrouch();
            SwitchState(AnimationState.Locomotion);
        }
    }
}