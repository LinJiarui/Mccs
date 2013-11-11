/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  RTSCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:54:36
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera is constrained to the limits of a plane. 
    /// This is the typical camera you can found in any Real Time Strategy game.
    /// </summary>
    public class RTSCameraMode : CameraMode
    {
        private Quaternion _rotation;
        private Radian _cameraPitch;
        private Vector3 _upAxis;
        private Vector3 _leftAxis;
        private Vector3 _heightAxis;
        private float _moveFactor;
        private float _heightDisplacement;
        private float _lateralDisplacement;
        private float _verticalDisplacement;
        private float _zoom;
        private float _minZoom;
        private float _maxZoom;

        public RTSCameraMode(CameraControlSystem cam, Vector3 initialPosition, Vector3 upAxis, Vector3 leftAxis, Radian cameraPitch, float minZoom = 0, float maxZoom = 0)
            : base(cam)
        {
            _upAxis = upAxis.NormalisedCopy;
            _leftAxis = leftAxis.NormalisedCopy;
            _moveFactor = 1;
            _cameraPitch = cameraPitch;
            _minZoom = minZoom;
            _maxZoom = maxZoom;
            _zoom = 0;

            _rotation = new Quaternion(cameraPitch, leftAxis);
            _heightAxis = -upAxis.CrossProduct(leftAxis).NormalisedCopy;
            CameraPosition = initialPosition;

            _heightDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;
        }

        public override bool Init()
        {
            //todo cameraMode.Init()
            base.Init();

            CameraCS.SetFixedYawAxis(false, Vector3.UNIT_Y);
            CameraCS.AutoTrackingTarget = false;

            CameraOrientation = _rotation;

            InstantUpdate();
            return true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException("camera mode: " + this.GetType().Name);
        }

        public override void Update(float timeSinceLastFrame)
        {
            var displacement = ((_upAxis * _verticalDisplacement)
                + (_heightAxis * _heightDisplacement)
                + (_leftAxis * _lateralDisplacement)) * timeSinceLastFrame * _moveFactor;

            CameraPosition += displacement;

            _heightDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;

            CameraOrientation = _rotation;
        }

        public override void InstantUpdate()
        {
            var displacement = ((_upAxis * _verticalDisplacement)
               + (_heightAxis * _heightDisplacement)
               + (_leftAxis * _lateralDisplacement));

            CameraPosition += displacement;

            _heightDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;

            CameraOrientation = _rotation;
        }

        public Radian CameraPitch
        {
            get { return _cameraPitch; }
            set
            {
                _cameraPitch = value;
                _rotation = new Quaternion(_cameraPitch, _leftAxis);
            }
        }

        /// <summary>
        /// brief Set the moving speed factor (increase the height)
        /// param unitsPerSecond the units the camera will be translated per second
        /// </summary>
        public float MoveFactor
        {
            get { return _moveFactor; }
            set
            {
                _moveFactor = value;
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                float desiredDisplacement = value - _zoom;

                //Ogre::Real desiredZoom = _zoom + desiredDisplacement;

                if (value <= _maxZoom && value >= _minZoom) {
                    _heightDisplacement = desiredDisplacement;
                    _zoom = value;
                } else if (value > _maxZoom) {
                    _heightDisplacement = _maxZoom - _zoom;
                    _zoom = _maxZoom;
                } else if (value < _minZoom) {
                    _heightDisplacement = _minZoom - _zoom;
                    _zoom = _minZoom;
                }

                InstantUpdate();
            }
        }

        /// <summary>
        ///  Tell the camera to zoom in (reduce the height)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void ZoomIn(float percentage = 1)
        {
            float desiredDisplacement = _moveFactor * percentage;
            float desiredZoom = _zoom + desiredDisplacement;

            Zoom = desiredZoom;

            /*if(desiredZoom <= _maxZoom && desiredZoom >= _minZoom)
            {
                _heightDisplacement = desiredDisplacement;
                _zoom = desiredZoom;
            }
            else if(desiredZoom > _maxZoom)
            {
                _heightDisplacement = _maxZoom - _zoom;
                _zoom = _maxZoom;
            }
            else if(desiredZoom < _minZoom)
            {
                _heightDisplacement = _minZoom - _zoom;
                _zoom = _minZoom;
            }*/
        }

        /// <summary>
        /// Tell the camera to zoom out
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void ZoomOut(float percentage = 1) { ZoomIn(-percentage); }
        
        /// <summary>
        /// Tell the camera to go right (along the leftAxis)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoRight(float percentage = 1) { _lateralDisplacement -= _moveFactor * percentage; }

        /// <summary>
        /// Tell the camera to go left (along the leftAxis)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoLeft(float percentage = 1) { GoRight(-percentage); }

        /// <summary>
        /// Tell the camera to go up (along the upAxis)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoUp(float percentage = 1) { _verticalDisplacement += _moveFactor * percentage; }

        /// <summary>
        /// Tell the camera to go down (along the upAxis)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoDown(float percentage = 1) { GoUp(-percentage); }
    }
}
