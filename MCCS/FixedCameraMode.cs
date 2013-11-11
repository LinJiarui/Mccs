/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  FixedCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 7:44:52
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera position and orientation never change. 
    /// </summary>
    public class FixedCameraMode : CameraMode
    {
        private Vector3 _fixedAxis;
        private Vector3 _lastPosition;
        private Quaternion _lastOrientation;

        public FixedCameraMode(CameraControlSystem cam, Vector3 fixedAxis)
            : base(cam)
        {
            _fixedAxis = fixedAxis;
            _lastPosition = Vector3.ZERO;
            _lastOrientation = Quaternion.IDENTITY;
        }

        public override void Dispose() { }

        public override bool Init()
        {
            base.Init();
            CameraCS.SetFixedYawAxis(true, _fixedAxis);
            CameraCS.AutoTrackingTarget=false;

            InstantUpdate();
            return true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException("camera mode: " + this.GetType().Name);
        }

        public override void Update(float timeSinceLastFrame) { }

        public override void InstantUpdate()
        {
            CameraPosition = _lastPosition;
            CameraOrientation = _lastOrientation;
        }

        public virtual void SetCameraPosition(Vector3 pos)
        {
            _lastPosition = pos;
            CameraPosition = pos;
        }

        public virtual void SetCameraOrientation(Quaternion orient)
        {
            _lastOrientation = orient;
            CameraOrientation = orient;
        }

        public virtual void SetCameraOrientation(Radian roll, Radian yaw, Radian pitch)
        {
            _lastOrientation = new Quaternion(roll, Vector3.UNIT_Z)
                * new Quaternion(yaw, Vector3.UNIT_Y)
                * new Quaternion(pitch, Vector3.UNIT_X);
            CameraOrientation = _lastOrientation;
        }

        public Vector3 FixedAxis { get { return _fixedAxis; }internal set { _fixedAxis = value; }}
    }
}
