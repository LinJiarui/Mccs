/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  FixedDirectionCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:47:56
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera is controlled by the user. In this mode the 
    /// target is always seen from the same point of view.
    /// </summary>
    public class FixedDirectionCameraMode : CameraModeWithTightness
    {
        protected Vector3 _fixedAxis;
        protected float _distance;
        protected Vector3 _direction;

        public FixedDirectionCameraMode(CameraControlSystem cam, Vector3 direction, float distance, Vector3 fixedAxis)
            : base(cam)
        {
            _fixedAxis = fixedAxis;
            _direction = direction.NormalisedCopy;
            _distance = distance;

            CameraTightness = 1;
        }

        public override bool Init()
        {
            //todo Init() should use CameraMode.Init()
            //CameraMode.Init();
            CameraPosition = CameraCS.CameraPosition;
            CameraOrientation = CameraCS.CameraOrientation;

            CameraCS.SetFixedYawAxis(true, _fixedAxis);
            CameraCS.AutoTrackingTarget=true;

            InstantUpdate();
            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            var cameraCurrentPosition = CameraCS.CameraPosition;
            var cameraFinalPositionIfNoTightness = CameraCS.CameraTargetPosition - _direction * _distance;

            var diff = (cameraFinalPositionIfNoTightness - cameraCurrentPosition) * CameraTightness;
            CameraPosition += diff;
        }

        public override void InstantUpdate()
        {
            CameraPosition = CameraCS.CameraTargetPosition - _direction * _distance;
        }

        public virtual void SetDirection(Vector3 direction)
        {
            _direction = direction.NormalisedCopy;
            InstantUpdate();
        }

        public virtual void SetDistance(float distance)
        {
            _distance = distance;
            InstantUpdate();
        }
    }
}
