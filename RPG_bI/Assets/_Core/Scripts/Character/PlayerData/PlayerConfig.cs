using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "RPG/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Player Locomotion")]
        public bool AlwaysStrafe = true;
        public float WalkSpeed = 1.4f;
        public float RunSpeed = 2.5f;
        public float SprintSpeed = 7f;
        public float SpeedChangeDamping = 10f;
        public float RotationSmoothing = 10f;
        public float CameraRotationOffset;

        [Header("Shuffles")]
        public float ButtonHoldThreshold = 0.15f;

        [Header("Capsule Values")]
        public float CapsuleStandingHeight = 1.8f;
        public float CapsuleStandingCentre = 0.93f;
        public float CapsuleCrouchingHeight = 1.2f;
        public float CapsuleCrouchingCentre = 0.6f;

        [Header("Player Strafing")]
        public float ForwardStrafeMinThreshold = -55.0f;
        public float ForwardStrafeMaxThreshold = 125.0f;

        [Header("Grounded Angle")]
        public LayerMask GroundLayerMask;
        public float GroundedOffset = -0.14f;

        [Header("Player In-Air")]
        public float JumpForce = 10f;
        public float GravityMultiplier = 2f;

        [Header("Player Head Look")]
        public AnimationCurve HeadLookXCurve;

        [Header("Player Body Look")]
        public AnimationCurve BodyLookXCurve;

        [Header("Player Lean")]
        public AnimationCurve LeanCurve;

        [Header("Animation Constants")]
        public float AnimationDampTime = 5f;
        public float StrafeDirectionDampTime = 20f;
    }
}