/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  DummyCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 23:32:39
 *           modified by
 * ***********************************************/

using Mccs;
using Mogre;

namespace CCS
{
    /// <summary>
    /// This is an example of how a custom camera mode can be implemented
    /// </summary>
    public class DummyCameraMode : CameraMode
    {
        private float _speed;
        private float _currentDirection;
        private float _directionWhenStopped;

        public DummyCameraMode(CameraControlSystem cam, float speed = 1)
            : base(cam)
        {
            _speed = speed;
            _directionWhenStopped = 1;
        }

        public override bool Init()
        {
            CameraCS.SetFixedYawAxis(false, Vector3.UNIT_Y);
            CameraCS.AutoTrackingTarget=false;

            _currentDirection = _directionWhenStopped;

            InstantUpdate();

            return true;
        }

        public override void Stop()
        {
            _directionWhenStopped = _currentDirection;
        }


        public override void Update(float timeSinceLastFrame)
        {
            Vector3 currentPosition = CameraCS.CameraSceneNode.Position;

            if (currentPosition.z > 1000) _currentDirection = -1;
            else if (currentPosition.z < -1000) _currentDirection = 1;

            CameraPosition += new Vector3(0, 0, _speed * _currentDirection * timeSinceLastFrame);
            //CameraOrient = Ogre::Quaternion::IDENTITY;
        }

        public override void InstantUpdate()
        {
            CameraPosition = Vector3.ZERO;
            CameraOrientation = Quaternion.IDENTITY;
        }
    }
}
