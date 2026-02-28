using UnityEngine;

namespace Character.AnimationController 
{
    public class AnimatorController : MonoBehaviour
    {
        private Animator _animator;

        private readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
        private readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
        private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
        private readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
        private readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");

        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");

        private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
        private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");

        private readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");

        private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");

        private readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
        private readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
        private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
        private readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");

        private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");

        private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
        private readonly int _isStartingHash = Animator.StringToHash("IsStarting");

        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

        private readonly int _leanValueHash = Animator.StringToHash("LeanValue");
        private readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
        private readonly int _headLookYHash = Animator.StringToHash("HeadLookY");

        private readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
        private readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");

        private readonly int _locomotionStartDirectionHash = Animator.StringToHash("LocomotionStartDirection");

        public AnimatorController(Animator animator)
        {
            _animator = animator;
        }

        public void UpdateAnimatorParameters(AnimatorData data)
        {
            _animator.SetFloat(_leanValueHash, data.LeanValue);
            _animator.SetFloat(_headLookXHash, data.HeadLookX);
            _animator.SetFloat(_headLookYHash, data.HeadLookY);
            _animator.SetFloat(_bodyLookXHash, data.BodyLookX);
            _animator.SetFloat(_bodyLookYHash, data.BodyLookY);

            _animator.SetFloat(_isStrafingHash, data.IsStrafing ? 1.0f : 0.0f);

            _animator.SetFloat(_inclineAngleHash, data.InclineAngle);

            _animator.SetFloat(_moveSpeedHash, data.Speed2D);
            _animator.SetInteger(_currentGaitHash, (int)data.CurrentGait);

            _animator.SetFloat(_strafeDirectionXHash, data.StrafeDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, data.StrafeDirectionZ);
            _animator.SetFloat(_forwardStrafeHash, data.ForwardStrafe);
            _animator.SetFloat(_cameraRotationOffsetHash, data.CameraRotationOffset);

            _animator.SetBool(_movementInputHeldHash, data.MovementInputHeld);
            _animator.SetBool(_movementInputPressedHash, data.MovementInputPressed);
            _animator.SetBool(_movementInputTappedHash, data.MovementInputTapped);
            _animator.SetFloat(_shuffleDirectionXHash, data.ShuffleDirectionX);
            _animator.SetFloat(_shuffleDirectionZHash, data.ShuffleDirectionZ);

            _animator.SetBool(_isTurningInPlaceHash, data.IsTurningInPlace);
            _animator.SetBool(_isCrouchingHash, data.IsCrouching);

            _animator.SetFloat(_fallingDurationHash, data.FallingDuration);
            _animator.SetBool(_isGroundedHash, data.IsGrounded);

            _animator.SetBool(_isWalkingHash, data.IsWalking);
            _animator.SetBool(_isStoppedHash, data.IsStopped);

            _animator.SetFloat(_locomotionStartDirectionHash, data.LocomotionStartDirection);
        }

        public void SetIsJumping(bool value)
        {
            _animator.SetBool(_isJumpingAnimHash, value);
        }

        public void SetLocomotionStartDirection(float value)
        {
            _animator.SetFloat(_locomotionStartDirectionHash, value);
        }

        public struct AnimatorData
        {
            public float LeanValue;
            public float HeadLookX;
            public float HeadLookY;
            public float BodyLookX;
            public float BodyLookY;
            public bool IsStrafing;
            public float InclineAngle;
            public float Speed2D;
            public GaitState CurrentGait;
            public float StrafeDirectionX;
            public float StrafeDirectionZ;
            public float ForwardStrafe;
            public float CameraRotationOffset;
            public bool MovementInputHeld;
            public bool MovementInputPressed;
            public bool MovementInputTapped;
            public float ShuffleDirectionX;
            public float ShuffleDirectionZ;
            public bool IsTurningInPlace;
            public bool IsCrouching;
            public float FallingDuration;
            public bool IsGrounded;
            public bool IsWalking;
            public bool IsStopped;
            public float LocomotionStartDirection;
        }

        public enum GaitState
        {
            Idle,
            Walk,
            Run,
            Sprint
        }
    }
}