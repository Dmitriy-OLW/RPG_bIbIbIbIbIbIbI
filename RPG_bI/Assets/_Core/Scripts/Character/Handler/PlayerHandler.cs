using Character.AnimationController;
using Character.InputController;
using CharacterCamera;
using UnityEngine;

namespace Character
{
    public class PlayerHandler : MonoBehaviour
    {
        [Header("External Components")]
        [SerializeField] private Animator _characterAnimator;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private Transform _rearRayPos;
        [SerializeField] private Transform _frontRayPos;

        private AnimatorController _animatorController;
        private PlayerStateMachine _stateMachine;
        private PlayerMovement _playerMovement;
        private PlayerRotation _playerRotation;
        private PlayerTargeting _playerTargeting;
        private PlayerCrouch _playerCrouch;
        private PlayerLocomotionAdditives _playerLocomotionAdditives;
        private PlayerGroundedChecker _playerGroundedChecker;
        private PlayerInputHandler _playerInputHandler;

        public CameraController CameraController => _cameraController;
        public InputReader InputReader => _inputReader;
        public CharacterController Controller => _controller;
        public AnimatorController AnimatorController => _animatorController;
        public PlayerConfig Config => _config;
        public Transform RearRayPos => _rearRayPos;
        public Transform FrontRayPos => _frontRayPos;
        
        public PlayerMovement PlayerMovement => _playerMovement;
        public PlayerRotation PlayerRotation => _playerRotation;
        public PlayerTargeting PlayerTargeting => _playerTargeting;
        public PlayerCrouch PlayerCrouch => _playerCrouch;
        public PlayerLocomotionAdditives PlayerLocomotionAdditives => _playerLocomotionAdditives;
        public PlayerGroundedChecker PlayerGroundedChecker => _playerGroundedChecker;

        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
        }

        private void InitializeComponents()
        {
            _animatorController = new AnimatorController(_characterAnimator);
            _playerGroundedChecker = new PlayerGroundedChecker(this);
            _playerMovement = new PlayerMovement(this);
            _playerRotation = new PlayerRotation(this);
            _playerTargeting = new PlayerTargeting(this);
            _playerCrouch = new PlayerCrouch(this);
            _playerLocomotionAdditives = new PlayerLocomotionAdditives(this);
            _playerInputHandler = new PlayerInputHandler(this);
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new PlayerStateMachine(this);
            
            var baseState = new PlayerBaseState(_stateMachine, this);
            var locomotionState = new PlayerLocomotionState(_stateMachine, this);
            var jumpState = new PlayerJumpState(_stateMachine, this);
            var fallState = new PlayerFallState(_stateMachine, this);
            var crouchState = new PlayerCrouchState(_stateMachine, this);
            
            _stateMachine.RegisterState(PlayerState.Base, baseState);
            _stateMachine.RegisterState(PlayerState.Locomotion, locomotionState);
            _stateMachine.RegisterState(PlayerState.Jump, jumpState);
            _stateMachine.RegisterState(PlayerState.Fall, fallState);
            _stateMachine.RegisterState(PlayerState.Crouch, crouchState);
            
            _stateMachine.SwitchState(PlayerState.Locomotion);
        }

        private void Start()
        {
            _playerInputHandler.SubscribeToInputEvents();
            _playerTargeting.Initialize();
        }

        private void Update()
        {
            _stateMachine.Update();
            _playerTargeting.UpdateBestTarget();
        }

        private void OnDestroy()
        {
            _playerInputHandler.UnsubscribeFromInputEvents();
        }
    }
}