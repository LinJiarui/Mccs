/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  AttachedCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:01:54
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera node is attached to the target node as a child
    /// </summary>
    public class AttachedCameraMode : CameraMode
    {
        private Quaternion _rotation;
        private Vector3 _relativePositionToCameraTarget;
        private Node _cameraPreviousParentSceneNode;

        public AttachedCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Quaternion rotation)
            : base(cam)
        {
            _rotation = rotation;
            _relativePositionToCameraTarget = relativePositionToCameraTarget;
        }

        public AttachedCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Radian roll, Radian yaw, Radian pitch)
            : this(cam, relativePositionToCameraTarget, new Quaternion(roll, Vector3.UNIT_Z) * new Quaternion(yaw, Vector3.UNIT_Y) * new Quaternion(pitch, Vector3.UNIT_X))
        {

        }

        public override bool Init()
        {
            //todo use CameraMode.Init() here
            //CameraMode.Init();
            CameraPosition = CameraCS.CameraPosition;
            CameraOrientation = CameraCS.CameraOrientation;

            CameraCS.SetFixedYawAxis(false, Vector3.UNIT_Y);
            CameraCS.AutoTrackingTarget=false;

            _cameraPreviousParentSceneNode = CameraCS.CameraSceneNode.Parent;
            _cameraPreviousParentSceneNode.RemoveChild(CameraCS.CameraSceneNode);
            CameraCS.TargetNode.AddChild(CameraCS.CameraSceneNode);

            CameraPosition = _relativePositionToCameraTarget;
            CameraOrientation = _rotation;

            InstantUpdate();

            return true;
        }

        public override void Stop()
        {
            //base.Stop();
            CameraCS.CameraSceneNode.Parent.RemoveChild(CameraCS.CameraSceneNode);
            _cameraPreviousParentSceneNode.AddChild(CameraCS.CameraSceneNode);
        }

        public override void Update(float timeSinceLastFrame)
        {
            //base.Update(timeSinceLastFrame);
        }

        public override void InstantUpdate()
        {
            //base.InstantUpdate();
        }
    }
}
