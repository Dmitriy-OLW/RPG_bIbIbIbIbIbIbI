using UnityEngine;
using Character.InputController;

namespace CharacterCamera
{
    public class CameraController : MonoBehaviour
    {
        private const int LagDeltaTimeAdjustment = 20;
        
        [Header("Targets")]
        [SerializeField] private GameObject _character;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _lockOnTarget;

        [Header("Settings")]
        [SerializeField] private bool _invertCamera;
        [SerializeField] private bool _hideCursor;
        [SerializeField] private bool _isLockedOn;
        [SerializeField] private float _mouseSensitivity = 5f;
        [SerializeField] private float _cameraDistance = 5f;
        [Space]
        [SerializeField] private float _cameraHeightOffset;
        [SerializeField] private float _cameraHorizontalOffset;
        [SerializeField] private float _cameraTiltOffset;
        [SerializeField] private Vector2 _cameraTiltBounds = new Vector2(-10f, 45f);
        [Space]
        [SerializeField] private float _positionalCameraLag = 1f;
        [SerializeField] private float _rotationalCameraLag = 1f;
        
        private float _cameraInversion;
        private float _lastAngleX;
        private float _lastAngleY;
        private Vector3 _lastPosition;
        private float _newAngleX;
        private float _newAngleY;
        private Vector3 _newPosition;
        private float _deltaRotationX;
        private float _deltaRotationY;
        
        private InputReader _inputReader;
        
        private void Start()
        {
            _inputReader = _character.GetComponent<InputReader>();

            if (_hideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _cameraInversion = _invertCamera ? 1 : -1;

            transform.position = _playerTarget.position;
            transform.rotation = _playerTarget.rotation;

            _lastPosition = transform.position;
            
            UpdateCameraTransformPosition();
        }
        
        private void Update()
        {
            float positionalFollowSpeed = CalculateFollowSpeed(_positionalCameraLag);
            float rotationalFollowSpeed = CalculateFollowSpeed(_rotationalCameraLag);

            UpdateDeltaRotations();
            HandleCameraRotation(rotationalFollowSpeed);
            HandleCameraPosition(positionalFollowSpeed);
            UpdateCameraTransformPosition();
            UpdateLastFrameData();
        }
        
        public void LockOn(bool enable, Transform newLockOnTarget)
        {
            _isLockedOn = enable;
            
            if (newLockOnTarget == null)
            {
                return;
            }

            _lockOnTarget = newLockOnTarget;
        }

        public Vector3 GetCameraPosition()
        {
            return _mainCamera.transform.position;
        }

        public Vector3 GetCameraForward()
        {
            return _mainCamera.transform.forward;
        }

        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            return GetCameraForwardZeroedY().normalized;
        }

        public Vector3 GetCameraRightZeroedYNormalised()
        {
            return GetCameraRightZeroedY().normalized;
        }
        
        public float GetCameraTiltX()
        {
            return _mainCamera.transform.eulerAngles.x;
        }
        
        private Vector3 GetCameraForwardZeroedY()
        {
            return new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z);
        }
        
        private Vector3 GetCameraRightZeroedY()
        {
            return new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z);
        }
        
        private float CalculateFollowSpeed(float lag)
        {
            return 1 / (lag / LagDeltaTimeAdjustment);
        }
        
        private void UpdateDeltaRotations()
        {
            _deltaRotationX = _inputReader.MouseDelta.y * _cameraInversion * _mouseSensitivity;
            _deltaRotationY = _inputReader.MouseDelta.x * _mouseSensitivity;
        }
        
        private void HandleCameraRotation(float rotationalFollowSpeed)
        {
            _newAngleX += _deltaRotationX;
            _newAngleX = Mathf.Clamp(_newAngleX, _cameraTiltBounds.x, _cameraTiltBounds.y);
            _newAngleX = Mathf.Lerp(_lastAngleX, _newAngleX, rotationalFollowSpeed * Time.deltaTime);

            if (_isLockedOn)
            {
                Vector3 aimVector = _lockOnTarget.position - _playerTarget.position;
                Quaternion targetRotation = Quaternion.LookRotation(aimVector);
                targetRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationalFollowSpeed * Time.deltaTime);
                _newAngleY = targetRotation.eulerAngles.y;
            }
            else
            {
                _newAngleY += _deltaRotationY;
                _newAngleY = Mathf.Lerp(_lastAngleY, _newAngleY, rotationalFollowSpeed * Time.deltaTime);
            }

            transform.eulerAngles = new Vector3(_newAngleX, _newAngleY, 0);
        }
        
        private void HandleCameraPosition(float positionalFollowSpeed)
        {
            _newPosition = _playerTarget.position;
            _newPosition = Vector3.Lerp(_lastPosition, _newPosition, positionalFollowSpeed * Time.deltaTime);
            transform.position = _newPosition;
        }
        
        private void UpdateCameraTransformPosition()
        {
            _cameraTransform.localPosition = new Vector3(_cameraHorizontalOffset, _cameraHeightOffset, _cameraDistance * -1);
            _cameraTransform.localEulerAngles = new Vector3(_cameraTiltOffset, 0f, 0f);
        }
        
        private void UpdateLastFrameData()
        {
            _lastPosition = _newPosition;
            _lastAngleX = _newAngleX;
            _lastAngleY = _newAngleY;
        }
    }
}