/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  FixedTrackingCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 7:55:41
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera position is fixed and the camera always 
    ///  points to the target. 
    /// </summary>
    public class FixedTrackingCameraMode : FixedCameraMode
    {
        public FixedTrackingCameraMode(CameraControlSystem cam, Vector3 fixedAxis)
            : base(cam, fixedAxis)
        { }

        public override bool Init()
        {
            //todo Init() should use CameraMode.Init()
            //CameraMode.Init();
            CameraPosition = CameraCS.CameraPosition;
            CameraOrientation = CameraCS.CameraOrientation;

            CameraCS.SetFixedYawAxis(true, FixedAxis);
            CameraCS.AutoTrackingTarget = true;

            InstantUpdate();
            return true;
        }
    }
}
