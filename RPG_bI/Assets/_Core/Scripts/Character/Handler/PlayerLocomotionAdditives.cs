using UnityEngine;

namespace Character
{
    public class PlayerLocomotionAdditives
    {
        private PlayerHandler _handler;
        
        private float _leanValue;
        private float _headLookX;
        private float _headLookY;
        private float _bodyLookX;
        private float _bodyLookY;
        
        private float _leanDelay;
        private float _headLookDelay;
        private float _bodyLookDelay;
        
        private bool _enableLean = true;
        private bool _enableHeadTurn = true;
        private bool _enableBodyTurn = true;
        
        private float _rotationRate;
        private float _initialLeanValue;
        private float _initialTurnValue;

        public float LeanValue => _leanValue;
        public float HeadLookX => _headLookX;
        public float HeadLookY => _headLookY;
        public float BodyLookX => _bodyLookX;
        public float BodyLookY => _bodyLookY;
        public bool EnableLean => _enableLean;
        public bool EnableHeadTurn => _enableHeadTurn;
        public bool EnableBodyTurn => _enableBodyTurn;

        public PlayerLocomotionAdditives(PlayerHandler handler)
        {
            _handler = handler;
        }

        public void SetDelays(float delayTime)
        {
            _leanDelay = delayTime;
            _headLookDelay = delayTime;
            _bodyLookDelay = delayTime;
        }

        public void CheckEnableTurns()
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_handler.PlayerMovement.IsStarting;
            
            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_handler.PlayerMovement.IsStarting || _handler.PlayerRotation.IsTurningInPlace);
        }

        public void CheckEnableLean()
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_handler.PlayerMovement.IsStarting || _handler.PlayerRotation.IsTurningInPlace);
        }

        public void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _handler.PlayerRotation.UpdateCurrentRotation();
                _rotationRate = _handler.PlayerRotation.GetRotationRate();
            }

            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float referenceValue = _handler.PlayerMovement.Speed2D / _handler.Config.SprintSpeed;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _handler.Config.LeanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;

            if (headLookActivated && _handler.PlayerRotation.IsTurningInPlace)
            {
                _initialTurnValue = _handler.Config.CameraRotationOffset;
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
                    _handler.Config.HeadLookXCurve,
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
                _handler.Config.BodyLookXCurve,
                _bodyLookX,
                false
            );

            float cameraTilt = _handler.CameraController.GetCameraTiltX();
            cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
            cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _handler.PlayerRotation.StorePreviousRotation();
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
    }
}