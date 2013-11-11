/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  CameraMode
 * history:  created by LinJiarui 2013/10/19 星期六 19:31:17
 *           modified by
 * ***********************************************/

using System;
using Mogre;

namespace Mccs
{
    public abstract class CameraMode : IDisposable
    {
        public CameraControlSystem CameraCS;
        private Vector3 _cameraPos;
        private Quaternion _cameraOrient;

        public CameraMode(CameraControlSystem cam)
        {
            CameraCS = cam;
            _cameraPos = Vector3.ZERO;
            _cameraOrient = Quaternion.IDENTITY;
        }

        public virtual void Dispose()
        {
            //todo
        }

        public virtual bool Init()
        {
            _cameraPos = CameraCS.CameraPosition;
            _cameraOrient = CameraCS.CameraOrientation;
            return true;
        }

        public abstract void Stop();
        public abstract void Update(float timeSinceLastFrame);
        public abstract void InstantUpdate();

        public Vector3 CameraPosition { get { return _cameraPos; }  set { _cameraPos = value; } }
        public Quaternion CameraOrientation { get { return _cameraOrient; }  set { _cameraOrient = value; } }
    }
}
