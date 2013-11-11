/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  ThroughTargetCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:30:35
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera points to a given position (the "focus") 
    /// throuh the target. The camera orientation is fixed by a yaw axis.
    /// </summary>
    public class ThroughTargetCameraMode : CameraMode
    {
        private Vector3 _fixedAxis;
        private Vector3 _focusPos;
        private float _margin;
        private bool _inverse;
        
        public Vector3 CameraFocusPosition
        {
            get { return _focusPos; }
            set
            {
                _focusPos = value;
                Init();
            }
        }
        public bool Inverse { get { return _inverse; } set { _inverse = value; } }
        
        public ThroughTargetCameraMode(CameraControlSystem cam, float margin, Vector3 focusPos, Vector3 fixedAxis, bool inverse = false)
            : base(cam)
        {
            _fixedAxis = fixedAxis;
            _margin = margin;
            _inverse = inverse;

            _focusPos = focusPos;
        }

        public override bool Init()
        {
            //todo CamerMode.Init()
            base.Init();

            CameraCS.SetFixedYawAxis(true, _fixedAxis);
            CameraCS.AutoTrackingTarget = true;

            InstantUpdate();
            return true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException("camera mode: " + this.GetType().Name);
        }

        public override void Update(float timeSinceLastFrame)
        {
            InstantUpdate();
        }

        public override void InstantUpdate()
        {
            if (CameraCS.HasCameraTarget) {
                if (!_inverse) {
                    var focusToTarget = CameraCS.CameraTargetPosition - _focusPos;
                    CameraPosition = CameraCS.CameraTargetPosition + focusToTarget.NormalisedCopy * _margin;
                } else {
                    var focusToTarget = CameraCS.CameraTargetPosition - _focusPos;
                    CameraPosition = _focusPos - focusToTarget.NormalisedCopy * _margin;
                }
            }
        }

    }
}
