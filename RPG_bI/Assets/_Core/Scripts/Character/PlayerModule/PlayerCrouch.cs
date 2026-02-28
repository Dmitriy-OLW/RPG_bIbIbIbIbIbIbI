using UnityEngine;

namespace Character
{
    public class PlayerCrouch
    {
        private PlayerHandler _handler;
        
        private bool _isCrouching;
        private bool _cannotStandUp;
        private bool _crouchKeyPressed;
        private bool _isSliding;

        public bool IsCrouching => _isCrouching;
        public bool CannotStandUp => _cannotStandUp;
        public bool CrouchKeyPressed => _crouchKeyPressed;
        public bool IsSliding => _isSliding;

        public PlayerCrouch(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void SetCrouchKeyPressed(bool value)
        {
            _crouchKeyPressed = value;
        }

        public void ActivateCrouch()
        {
            _crouchKeyPressed = true;

            if (_handler.PlayerGroundedChecker.IsGrounded)
            {
                CapsuleCrouchingSize(true);
                _handler.PlayerRotation.SetIsSprinting(false);
                _isCrouching = true;
            }
        }

        public void DeactivateCrouch()
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

        public void CapsuleCrouchingSize(bool crouching)
        {
            if (crouching)
            {
                _handler.Controller.center = new Vector3(0f, _handler.Config.CapsuleCrouchingCentre, 0f);
                _handler.Controller.height = _handler.Config.CapsuleCrouchingHeight;
            }
            else
            {
                _handler.Controller.center = new Vector3(0f, _handler.Config.CapsuleStandingCentre, 0f);
                _handler.Controller.height = _handler.Config.CapsuleStandingHeight;
            }
        }

        public void CeilingHeightCheck()
        {
            float rayDistance = Mathf.Infinity;
            float minimumStandingHeight = _handler.Config.CapsuleStandingHeight - _handler.FrontRayPos.localPosition.y;

            Vector3 midpoint = new Vector3(_handler.transform.position.x, 
                _handler.transform.position.y + _handler.FrontRayPos.localPosition.y, 
                _handler.transform.position.z);
                
            if (Physics.Raycast(midpoint, _handler.transform.TransformDirection(Vector3.up), 
                out RaycastHit ceilingHit, rayDistance, _handler.Config.GroundLayerMask))
            {
                _cannotStandUp = ceilingHit.distance < minimumStandingHeight;
            }
            else
            {
                _cannotStandUp = false;
            }
        }
    }
}