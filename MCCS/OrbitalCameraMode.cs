/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  OrbitalCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 14:29:01
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// This is basically an attached camera mode where the user
    /// can mofify the camera position. If the scene focus is seen
    /// as the center of a sphere, the camera rotates arount it
    /// </summary>
    public class OrbitalCameraMode : ChaseFreeYawAxisCameraMode
    {
        private float _zoomFactor;
        private float _rotationFactor;
        private Radian _rotHorizontal;
        private Radian _rotVertical;
        private float _zoom;
        private Radian _initialRotHorizontal;
        private Radian _initialRotVertical;
        private float _initialZoom;
        private Radian _rotHorizontalDisplacement;
        private Radian _rotVerticalDisplacement;
        private float _zoomDisplacement;
        private bool _resetToInitialPosition;

        public OrbitalCameraMode(CameraControlSystem cam, Radian initialHorizontalRotation,
             Radian initialVerticalRotation, float initialZoom = 1
             , bool resetToInitialPosition = true, float collisionmargin = 0.1f)
            : base(cam, Vector3.ZERO, new Radian(0), new Radian(0), new Radian(0), collisionmargin)
        {
            _zoomFactor = 1;
            _rotationFactor = 0.13f;
            _initialRotHorizontal = initialHorizontalRotation;
            _initialRotVertical = initialVerticalRotation;
            _initialZoom = initialZoom;
            _zoom = initialZoom;
            _rotHorizontal = 0;
            _rotVertical = 0;
            _zoomDisplacement = 0;
            _resetToInitialPosition = resetToInitialPosition;
            this.CameraTightness = 1;
        }


        public override bool Init()
        {
            //ChaseFreeYawAxisCameraMode::Init();
            base.Init();

            ((CameraMode)this).CameraCS.SetFixedYawAxis(false, Vector3.UNIT_Y);
            ((CameraMode)this).CameraCS.AutoTrackingTarget = false;

            if (_resetToInitialPosition) {
                _rotHorizontal = _initialRotHorizontal;
                _rotVertical = _initialRotVertical;
                _zoom = _initialZoom;
            }

            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            //ChaseFreeYawAxisCameraMode::Update(timeSinceLastFrame);
            base.Update(timeSinceLastFrame);

            _rotVertical += _rotVerticalDisplacement * timeSinceLastFrame * _rotationFactor;
            _rotHorizontal += _rotHorizontalDisplacement * timeSinceLastFrame * _rotationFactor;
            _zoom += _zoomDisplacement * timeSinceLastFrame * _zoomFactor;

            Quaternion offsetVertical = new Quaternion(_rotVertical, Vector3.UNIT_X);
            Quaternion offsetHorizontal = new Quaternion(_rotHorizontal, Vector3.UNIT_Y);

            CameraOrientation = CameraOrientation * offsetHorizontal * offsetVertical;
            CameraPosition += CameraOrientation * new Vector3(0, 0, _zoom);

            _rotVerticalDisplacement = 0;
            _rotHorizontalDisplacement = 0;
            _zoomDisplacement = 0;

            if (CollisionsEnabled) {
                CameraPosition = CollisionFunc(((CameraMode)this).CameraCS.CameraTargetPosition, CameraPosition);
            }
        }

        /// <summary>
        /// Set the zooming speed factor
        /// the units the camera will be zoomed in/out per second
        /// </summary>
        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        /// <summary>
        /// Set the rotating speed factor
        /// the radians the camera will be rotated per second
        /// </summary>
        public float RotationFactor { get { return _rotationFactor; } set { _rotationFactor = value; } }

        /// <summary>
        /// the amount of rotation (use negative values to look left)
        /// </summary>
        public Radian Yaw { get { return -_rotHorizontal; } set { _rotHorizontal = _initialRotHorizontal - value; } }

        /// <summary>
        /// the amount of rotation (use negative values to look up)
        /// </summary>
        public Radian Pitch { get { return -_rotVertical; } set { _rotVertical = _initialRotVertical - value; } }

        public float Zoom { get { return _zoom; } set { _zoom = value; } }
        /// <summary>
        /// Tell the camera to look right
        /// </summary>
        /// <param name="val">percentage of the speed factor [-1,1] (use negative values to look left)</param>
        public void DoYaw(float val = 1) { _rotHorizontalDisplacement += new Degree(-val); }

        /// <summary>
        /// Tell the camera to look down
        /// </summary>
        /// <param name="val">val percentage of the speed factor [-1,1] (use negative values to look up)</param>
        public void DoPitch(float val = 1) { _rotVerticalDisplacement += new Degree(-val); }

        /// <summary>
        /// Tell the camera to zoom out
        /// </summary>
        /// <param name="val">val percentage of the speed factor [-1,1] (use negative values to zoom in)</param>
        public void DoZoom(float val = 1) { _zoomDisplacement += val; }

        public void Reset()
        {
            _rotHorizontal = _initialRotHorizontal;
            _rotVertical = _initialRotVertical;
            _zoom = _initialZoom;
        }

    }
}
