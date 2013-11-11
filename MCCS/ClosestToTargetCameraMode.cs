/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  ClosestToTargetCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 8:38:17
 *           modified by
 * ***********************************************/

using System.Collections.Generic;
using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this camera mode the position of the camera is chosen to be 
    /// the closest to the target of a given list. The camera orientation 
    /// is fixed by a yaw axis.
    /// </summary>
    public class ClosestToTargetCameraMode : FixedTrackingCameraMode
    {
        private List<Vector3> _positionsList;
        private float _timeInterval;
        private float _time;
        private bool _inverse;

        public ClosestToTargetCameraMode(CameraControlSystem cam, Vector3 fixedAxis, float timeInterval = 1)
            : base(cam, fixedAxis)
        {
            _timeInterval = timeInterval;
            _time = timeInterval;

            _positionsList=new List<Vector3>();
        }

        public override void Update(float timeSinceLastFrame)
        {
            InstantUpdate();
            //todo how to deal with return of c++ here
            return;

            _time -= timeSinceLastFrame;
            if (_time < 0) {
                InstantUpdate();
                _time = _timeInterval;
            }
        }

        public override void InstantUpdate()
        {
            if (CameraCS.HasCameraTarget) {
                var minDistance = float.MaxValue;
                var targetPosition = CameraCS.CameraTargetPosition;
                Vector3 closest = CameraPosition;
                foreach (var it in _positionsList) {
                    var distance = (it - targetPosition).Length;
                    if (distance < minDistance) {
                        closest = it;
                        minDistance = distance;
                    }
                }

                CameraPosition = closest;
            }
        }

        public virtual void AddCameraPosition(Vector3 pos)
        {
            _positionsList.Add(pos);
            Init();
        }
    }
}
