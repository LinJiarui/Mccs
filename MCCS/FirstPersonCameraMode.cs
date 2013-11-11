/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  FirstPersonCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:18:40
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// This is basically an attached camera mode with the posibility of hide 
    /// or show the focus of the scene
    /// </summary>
    public class FirstPersonCameraMode : AttachedCameraMode
    {
        private bool _isCharacterVisible;

        public FirstPersonCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Quaternion rotation)
            : base(cam, relativePositionToCameraTarget, rotation)
        {
            _isCharacterVisible = true;
        }

        public FirstPersonCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Radian roll, Radian yaw, Radian pitch)
            : base(cam, relativePositionToCameraTarget, roll, yaw, pitch)
        {
            _isCharacterVisible = true;
        }

        public override bool Init()
        {
            base.Init();

            CameraCS.TargetNode.SetVisible(_isCharacterVisible);

            return true;
        }

        public override void Stop()
        {
            base.Stop();

            CameraCS.TargetNode.SetVisible(true);
        }

        public bool IsCharacterVisible
        {
            get { return _isCharacterVisible; }
            set
            {
                _isCharacterVisible = value;
                if (CameraCS.HasCameraTarget) {
                    CameraCS.TargetNode.SetVisible(_isCharacterVisible);
                }
            }
        }

    }
}
