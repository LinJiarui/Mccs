/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  ChaseFreeYawAxisCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 9:46:21
 *           modified by
 * ***********************************************/

using Mogre;

namespace Mccs
{
    /// <summary>
    /// This mode is similar to "Chase" camera mode but the camera orientation is not fixed by
    /// a yaw axis. The camera orientation will be the same as the target.
    /// </summary>
    public class ChaseFreeYawAxisCameraMode : ChaseCameraMode
    {
        private Quaternion _rotationOffset;

        public ChaseFreeYawAxisCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Radian roll, Radian yaw, Radian pitch, float collisionMargin = 0.1f)
            : base(cam, relativePositionToCameraTarget, Vector3.UNIT_Y, collisionMargin)
        {
            _rotationOffset = new Quaternion(roll, Vector3.UNIT_Z) * new Quaternion(yaw, Vector3.UNIT_Y) * new Quaternion(pitch, Vector3.UNIT_X);
        }

        public override bool Init()
        {
            //todo Init() should use CameraMode.Init()
            //CameraMode.Init();
            CameraPosition = ((CameraMode) this).CameraCS.CameraPosition;
            CameraOrientation = ((CameraMode) this).CameraCS.CameraOrientation;

            base.SetFixedYawAxis(false, Vector3.UNIT_Y);
            ((CameraMode) this).CameraCS.AutoTrackingTarget = false;

            this.CollisionFunc = this.DefaultCollisionDetectionFunction;

            InstantUpdate();

            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            // Update camera position
            base.Update(timeSinceLastFrame);

            // Update camera orientation
            CameraOrientation = ((CameraMode)this).CameraCS.CameraTargetOrientation * _rotationOffset;
        }

        public override void InstantUpdate()
        {
            if (null != ((CameraMode) this).CameraCS.TargetNode) {
                // Update camera position
                base.InstantUpdate();

                // Update camera orientation
                CameraOrientation = ((CameraMode)this).CameraCS.CameraTargetOrientation * _rotationOffset;
            }
        }

        // Specific methods

        public virtual void SetCameraRelativePosition(Vector3 posRelativeToCameraTarget, Quaternion rotation)
        {
            RelativePosToTarget = posRelativeToCameraTarget;

            _rotationOffset = rotation;

            InstantUpdate();
        }

        public virtual void SetCameraRelativePosition(Vector3 posRelativeToCameraTarget
            , Radian roll, Radian yaw, Radian pitch)
        {
            RelativePosToTarget = posRelativeToCameraTarget;

            _rotationOffset = new Quaternion(roll, Vector3.UNIT_Z)
                            * new Quaternion(yaw, Vector3.UNIT_Y)
                            * new Quaternion(pitch, Vector3.UNIT_X);

            InstantUpdate();
        }
    }
}
