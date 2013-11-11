/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  SphericCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 14:04:02
 *           modified by
 * ***********************************************/

using System;
using Mogre;

namespace Mccs
{
    /// <summary>
    /// The camera position is constrained to the extent of a sphere centered in the player (Zelda-style). 
    /// </summary>
    public class SphericCameraMode : CameraMode
    {
        private Vector3 _fixedAxis;
        private Vector3 _offset;
        private float _outerSphereRadius;
        private float _innerSphereRadius;
        private bool _resetDistance;
        private float _resetRotationFactor;
        private Vector3 _relativePositionToCameraTarget;
        private float _autoResetTime;
        private float _timeSinceLastChange;
        private bool _resseting;
        private Radian _LastRessetingDiff;
        private Vector3 _lastTargetPosition;
        private Quaternion _lastTargetOrientation;

        /**
		 * @param relativePositionToCameraTarget Default camera position with respecto to camera target
		 * @param autoResetTime If greater than zero the camera will be resetted to its initial position after the amount of seconds specified.
		 * @param resetDistance true for correcting the camera distance to the target when resetting. If false, the camera will maintaing the current distance to target.
		 * @param resetRotationFactor Speed factor for correcting the camera rotation when resetting.
		 * @param offset Offset applied to the center of the sphere.  
		 */
        public SphericCameraMode(CameraControlSystem cam
             , Vector3 relativePositionToCameraTarget
             , float outerSphereRadius
            , Vector3 offset
             , Vector3 fixedAxis, float innerSphereRadius = 0.0f
             , float autoResetTime = 3.0f
             , bool resetDistance = true
             , float resetRotationFactor = 1.0f)
            : base(cam)
        {
            _relativePositionToCameraTarget = relativePositionToCameraTarget;
            _outerSphereRadius = outerSphereRadius;
            _innerSphereRadius = innerSphereRadius;
            _autoResetTime = autoResetTime;
            _resetDistance = resetDistance;
            _resetRotationFactor = resetRotationFactor;
            _offset = offset;
            _fixedAxis = fixedAxis;
            _resseting = false;
            _LastRessetingDiff = new Radian(2.0f * Mogre.Math.PI);

            if (innerSphereRadius > outerSphereRadius) { throw new Exception("Inner radious greater than outer radious"); }
            if (innerSphereRadius > relativePositionToCameraTarget.Length
                || outerSphereRadius < relativePositionToCameraTarget.Length) {
                throw new Exception("relativePositionToCameraTarget param value not valid.");
            }
        }


        public override bool Init()
        {
            //CameraMode::Init();
            base.Init();

            CameraCS.SetFixedYawAxis(true, _fixedAxis);
            CameraCS.AutoTrackingTarget = true;

            if (CameraCS.HasCameraTarget) {
                _lastTargetPosition = CameraCS.CameraTargetPosition;
                _lastTargetOrientation = CameraCS.CameraTargetOrientation;

                Reset();
            } else {
                _lastTargetPosition = Vector3.ZERO;
                _lastTargetOrientation = Quaternion.IDENTITY;
            }

            _timeSinceLastChange = 0;

            return true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException("camera mode: " + this.GetType().Name);
        }

        public override void Update(float timeSinceLastFrame)
        {
            InstantUpdate();

            if (_autoResetTime > 0) {
                if (CameraCS.CameraTargetPosition == _lastTargetPosition
                    && CameraCS.CameraTargetOrientation == _lastTargetOrientation) {
                    _timeSinceLastChange += timeSinceLastFrame;
                    if (_timeSinceLastChange >= _autoResetTime) {
                        _timeSinceLastChange = 0;

                        Reset(false);
                    }
                } else {
                    _timeSinceLastChange = 0;
                    _resseting = false;
                }

                _lastTargetPosition = CameraCS.CameraTargetPosition;
                _lastTargetOrientation = CameraCS.CameraTargetOrientation;
            }

            if (_resseting) {
                Vector3 cameraTargetPosition = CameraCS.CameraTargetPosition;

                Vector3 cameraDirection = (CameraPosition - cameraTargetPosition) - _offset;

                Vector3 derivedRelativePositionToCameraTarget =
                    CameraCS.CameraTargetOrientation * _relativePositionToCameraTarget;

                Radian diffRot =
                    //derivedRelativePositionToCameraTarget.angleBetween(cameraDirection);
                Mogre.Math.ACos(derivedRelativePositionToCameraTarget.NormalisedCopy.DotProduct(cameraDirection.NormalisedCopy));
                float diffDist = cameraDirection.Length - _relativePositionToCameraTarget.Length;
                if ((diffRot.ValueRadians > 0 && diffRot < _LastRessetingDiff)
                    || (_resetDistance && diffDist != 0)) {
                    _LastRessetingDiff = diffRot;

                    Vector3 rotNormal = derivedRelativePositionToCameraTarget.CrossProduct(cameraDirection);
                    rotNormal.Normalise();

                    Radian deltaAngle = new Radian(timeSinceLastFrame * _resetRotationFactor * diffRot.ValueRadians);
                    Quaternion q = new Quaternion(
                        deltaAngle
                        , -rotNormal);

                    Vector3 newCameraDirection = q * cameraDirection;

                    if (_resetDistance && diffDist != 0) {
                        Vector3 newCameraDirectionNormalised = newCameraDirection.NormalisedCopy;
                        float distToBeReduced = 0;
                        if (diffRot.ValueRadians > 0) {
                            distToBeReduced = (deltaAngle.ValueRadians / diffRot.ValueRadians) * newCameraDirection.Length;
                        }

                        if (diffDist > 0) {
                            newCameraDirection = newCameraDirection
                                - (newCameraDirectionNormalised * distToBeReduced);

                            var newDiffDist = newCameraDirection.Length - _relativePositionToCameraTarget.Length;
                            if (newDiffDist < 0) {
                                newCameraDirection = newCameraDirectionNormalised * _relativePositionToCameraTarget.Length;
                            }
                        } else {
                            newCameraDirection = newCameraDirection
                                + (newCameraDirectionNormalised * distToBeReduced);

                            var newDiffDist = newCameraDirection.Length - _relativePositionToCameraTarget.Length;
                            if (newDiffDist > 0) {
                                newCameraDirection = newCameraDirectionNormalised * _relativePositionToCameraTarget.Length;
                            }
                        }
                    }

                    CameraPosition = (cameraTargetPosition + newCameraDirection) + _offset;
                } else {
                    _resseting = false;
                    _LastRessetingDiff = new Radian(2.0f * Mogre.Math.PI);
                }
            }
        }

        public override void InstantUpdate() { }

        // Specific methods

        /**
         * @brief Reset the camera position to its initial position.
         *
         * @param instant true for doing it instantaniously; false for doing it smoothly.
         */
        public void Reset(bool instant = true) { }

        public float OuterSphereRadius
        {
            get { return _outerSphereRadius; }
            set
            {
                if (value >= _innerSphereRadius && value >= _relativePositionToCameraTarget.Length) {
                    _outerSphereRadius = value;
                }
            }
        }

        public float InnerSphereRadius
        {
            get { return _innerSphereRadius; }
            set
            {
                if (value <= _outerSphereRadius && value <= _relativePositionToCameraTarget.Length) {
                    _innerSphereRadius = value;
                }
            }
        }

        public Vector3 Offset { get { return _offset; } set { _offset = value; } }
        /// <summary>
        /// Displacement of the center of the sphere with respect to the fixed axis
        /// </summary>
        public float HeightOffset { get { return _offset.Length ; } set { _offset = _fixedAxis.NormalisedCopy * value; } }

        //seconds zero for disabling
        public float AutoResetTime { get { return _autoResetTime; } set { _autoResetTime = value; } }

    }
}
