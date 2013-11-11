/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  OrbitalWithMouseCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 23:37:11
 *           modified by
 * ***********************************************/

using Mccs;
using Mogre;

namespace CCS
{
    /// <summary>
    /// This is a second example of how a custom camera mode can be implemented
    /// </summary>
    public class OrbitalWithMouseCameraMode : OrbitalCameraMode
    {
        private MOIS.MouseButtonID _activateButton;

        public OrbitalWithMouseCameraMode(CameraControlSystem cam
             , MOIS.MouseButtonID activateButton
             , Radian initialHorizontalRotation
             , Radian initialVerticalRotation, float initialZoom = 1)
            : base(cam, initialHorizontalRotation, initialVerticalRotation, initialZoom)
        {
            _activateButton = activateButton;
        }


        public bool InjectMouseMoved(MOIS.MouseEvent e)
        {
            if (e.state.ButtonDown(_activateButton)) {
                this.DoYaw(e.state.X.rel * 5);
                this.DoPitch(e.state.Y.rel * 5);
            } else if (e.state.ButtonDown(MOIS.MouseButtonID.MB_Middle)) {
                Pan(e.state.X.rel, e.state.Y.rel);
            }
            this.DoZoom(-e.state.Z.rel * 5);

            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            base.Update(timeSinceLastFrame);
            CameraPosition += new Vector3(v.x, 0, v.z);
            if (CameraCS.HasCameraTarget) {
                CameraCS.TargetNode.Position += new Vector3(v.x, 0, v.z);
                v = new Vector3(0, 0, 0);
            }
        }

        Mogre.Vector3 v = new Vector3(0, 0, 0);
        internal void Pan(float dx, float dz)
        {
            v = CameraCS.CameraOrientation * ((Mogre.Vector3.UNIT_Z * -dz) + (Mogre.Vector3.UNIT_X * -dx));
            //CameraPosition += new Vector3(v.x, 0, v.z);
        }
    }
}
