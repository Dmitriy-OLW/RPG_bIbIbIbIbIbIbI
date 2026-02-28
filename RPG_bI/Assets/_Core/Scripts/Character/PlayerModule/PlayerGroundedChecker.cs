using UnityEngine;

namespace Character
{
    public class PlayerGroundedChecker
    {
        private PlayerHandler _handler;
        
        private bool _isGrounded = true;
        private float _inclineAngle;

        public bool IsGrounded => _isGrounded;
        public float InclineAngle => _inclineAngle;

        public PlayerGroundedChecker(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _handler.Controller.transform.position.x,
                _handler.Controller.transform.position.y - _handler.Config.GroundedOffset,
                _handler.Controller.transform.position.z
            );
            _isGrounded = Physics.CheckSphere(spherePosition, _handler.Controller.radius, 
                _handler.Config.GroundLayerMask, QueryTriggerInteraction.Ignore);
        }

        public void GroundInclineCheck()
        {
            float rayDistance = Mathf.Infinity;
            _handler.RearRayPos.rotation = Quaternion.Euler(_handler.transform.rotation.x, 0, 0);
            _handler.FrontRayPos.rotation = Quaternion.Euler(_handler.transform.rotation.x, 0, 0);

            Physics.Raycast(_handler.RearRayPos.position, _handler.RearRayPos.TransformDirection(-Vector3.up), 
                out RaycastHit rearHit, rayDistance, _handler.Config.GroundLayerMask);
            Physics.Raycast(_handler.FrontRayPos.position, _handler.FrontRayPos.TransformDirection(-Vector3.up), 
                out RaycastHit frontHit, rayDistance, _handler.Config.GroundLayerMask);

            Vector3 hitDifference = frontHit.point - rearHit.point;
            float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

            _inclineAngle = Mathf.Lerp(_inclineAngle, 
                Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
        }
    }
}