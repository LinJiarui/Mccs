/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  CameraModeWithTightness
 * history:  created by LinJiarui 2013/10/19 星期六 19:42:20
 *           modified by
 * ***********************************************/

namespace Mccs
{
    public class CameraModeWithTightness : CameraMode
    {
        private float _tightness;

        public CameraModeWithTightness(CameraControlSystem cam)
            : base(cam)
        {
            _tightness = 1;
        }


        public float CameraTightness
        {
            get { return _tightness; }
            set { _tightness = value; }
        }

        #region Overrides of CameraMode

        public override void Stop()
        {
            //throw new NotImplementedException();
        }

        public override void Update(float timeSinceLastFrame)
        {
            //throw new NotImplementedException();
        }

        public override void InstantUpdate()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
