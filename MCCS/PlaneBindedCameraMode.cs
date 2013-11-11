/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  PlaneBindedCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:23:00
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera is constrained to the limits of a plane. 
    /// The camera always points to the target, perpendicularly to the plane.
    /// </summary>
    public class PlaneBindedCameraMode : CameraModeWithTightness
    {
        private Vector3 _fixedAxis;
        private Plane _plane;

        public PlaneBindedCameraMode(CameraControlSystem cam, Plane plane, Vector3 fixedAxis)
            : base(cam)
        {
            _fixedAxis = fixedAxis;
            _plane = plane;

            CameraTightness = 1;
        }

        public override bool Init()
        {
            //todo Init() should use CameraMode.Init()
            //CameraMode.Init();
            CameraPosition = CameraCS.CameraPosition;
            CameraOrientation = CameraCS.CameraOrientation;

            CameraCS.SetFixedYawAxis(true, _fixedAxis);
            CameraCS.AutoTrackingTarget = true;

            InstantUpdate();

            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            float distance = _plane.GetDistance(CameraCS.CameraTargetPosition);

            var cameraCurrentPosition = CameraCS.CameraPosition;
            var cameraFinalPositionIfNoTightness = CameraCS.CameraTargetPosition -
                                                   _plane.normal.NormalisedCopy * distance;

            var diff = (cameraFinalPositionIfNoTightness - cameraCurrentPosition) * CameraTightness;
            CameraPosition += diff;
        }

        public override void InstantUpdate()
        {
            float distance = _plane.GetDistance(CameraCS.CameraTargetPosition);
            CameraPosition = CameraCS.CameraTargetPosition - _plane.normal.NormalisedCopy * distance;
        }
    }
}
