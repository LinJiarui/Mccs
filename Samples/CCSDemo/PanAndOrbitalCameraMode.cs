/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  PanAndOrbitalCameraMode
 * history:  created by LinJiarui 2013/10/27 星期日 11:42:35
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CCS;
using Mogre;
using Vector3 = Mogre.Vector3;

namespace Samples
{
    /*
    public class PanAndOrbitalCameraMode : OrbitalCameraMode
    {
        public void InjectMouseState(MOIS.MouseEvent ms)
        {
            if (ms.state.ButtonDown(MOIS.MouseButtonID.MB_Left)) {
                Orbit(ms.state.X.rel, ms.state.Y.rel);
            }
            if (ms.state.ButtonDown(MOIS.MouseButtonID.MB_Middle)) {
                Pan(ms.state.X.rel, ms.state.Y.rel);
            }

        }

        internal void Pan(float dx, float dz)
        {
            var v = CameraCS.CameraOrientation * ((Mogre.Vector3.UNIT_Z * -dz) + (Mogre.Vector3.UNIT_X * -dx));
            CameraPosition += new Vector3(v.x, 0, v.z);
        }

        internal void Orbit(float dx, float dy)
        {
            var ori = CameraCS.CameraOrientation;

            Mogre.Quaternion q = new Quaternion();

            //camNode.Pitch(new Mogre.Radian(-dy), Mogre.Node.TransformSpace.TS_LOCAL);
            //camNode.Yaw(new Mogre.Radian(-dx), Mogre.Node.TransformSpace.TS_WORLD);
        }
    }

    //*/
}
