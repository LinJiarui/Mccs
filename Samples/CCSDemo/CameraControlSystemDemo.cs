/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  CameraControlSystemDemo
 * history:  created by LinJiarui 2013/10/19 星期六 19:22:21
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using CCS;
using MOIS;
using Mccs;
using Mogre;
using Vector3 = Mogre.Vector3;

namespace Samples.CCS
{
    public class CCSDemo : MyGameApp
    {
        private InputManager inputManager;
        private Keyboard inputKeyboard;
        private Mouse inputMouse;
        public const float PLANE_SIZE = 1000.0f;

        SceneNode headNode;
        SceneNode atheneNode;
        AnimationState mAnimState;

        CameraControlSystem mCameraCS;

        protected override void CreateFrameListeners()
        {
            base.CreateFrameListeners();
            CreateInput();
        }

        public void CreateInput()
        {
            MOIS.ParamList pl = new ParamList();
            IntPtr windHnd;
            this.mRenderWindow.GetCustomAttribute("WINDOW", out windHnd);
            pl.Insert("WINDOW", windHnd.ToString());
            //Non-exclusive input, show OS cursor
            //If you want to see the mouse cursor and be able to move it outside your OGRE window and use the keyboard outside the running application 
            pl.Insert("w32_mouse", "DISCL_FOREGROUND");
            pl.Insert("w32_mouse", "DISCL_NONEXCLUSIVE");
            pl.Insert("w32_keyboard", "DISCL_FOREGROUND");
            pl.Insert("w32_keyboard", "DISCL_NONEXCLUSIVE");
            inputManager = MOIS.InputManager.CreateInputSystem(pl);

            //Create all devices (except joystick, as most people have Keyboard/Mouse) using buffered input.
            inputKeyboard = (Keyboard)inputManager.CreateInputObject(MOIS.Type.OISKeyboard, true);
            inputMouse = (Mouse)inputManager.CreateInputObject(MOIS.Type.OISMouse, true);

            MOIS.MouseState_NativePtr mouseState = inputMouse.MouseState;
            mouseState.width = this.mViewport.ActualWidth;//update after resize window
            mouseState.height = this.mViewport.ActualHeight;

            CreateEventHandler();
        }

        public void CreateEventHandler()
        {
            this.mRoot.FrameStarted += FrameStarted;
            this.mRoot.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(FrameRenderingQueued);
            if (inputKeyboard != null) {
                LogManager.Singleton.LogMessage("Setting up keyboard listeners");
                //inputKeyboard.KeyPressed += new KeyListener.KeyPressedHandler(KeyPressed);
                //inputKeyboard.KeyReleased += new KeyListener.KeyReleasedHandler(KeyReleased);
            }

            if (inputMouse != null) {
                LogManager.Singleton.LogMessage("Setting up mouse listeners");
                inputMouse.MousePressed += new MOIS.MouseListener.MousePressedHandler(MousePressed);
                inputMouse.MouseReleased += new MOIS.MouseListener.MouseReleasedHandler(MouseReleased);
                inputMouse.MouseMoved += new MOIS.MouseListener.MouseMovedHandler(MouseMotion);
            }
        }

        private bool FrameRenderingQueued(FrameEvent evt)
        {
            if (mRenderWindow.IsClosed) {
                return false;
            }
            if (mCameraCS != null) {
                processPlayerKeyInput(evt);
                ProcessCameraInput();
                ProcessUnbufferedMouseInput();
                mCameraCS.Update(evt.timeSinceLastFrame);
            }

            return true;
        }

        private void ProcessUnbufferedMouseInput()
        {
            var ms = inputMouse.MouseState;
            CameraMode cameraMode = mCameraCS.CurrentCameraMode;
            String cameraModeName = mCameraCS.GetCameraModeName(cameraMode);

            if (cameraModeName == "Free") {
                FreeCameraMode freeCameraMode = (FreeCameraMode)cameraMode;

                freeCameraMode.DoYaw(ms.X.rel);
                freeCameraMode.DoPitch(ms.Y.rel);
            } else if (cameraModeName == "RTS") {
                RTSCameraMode rtsCameraMode = (RTSCameraMode)cameraMode;

                //                float borderWidthPercentage = 0.1f; // 10%
                float borderWidthPercentage = 0.2f; // 20%
                float leftBorder = ms.width * borderWidthPercentage;
                float rightBorder = ms.width - (ms.width * borderWidthPercentage);
                float topBorder = ms.height * borderWidthPercentage;
                float bottomBorder = ms.height - (ms.height * borderWidthPercentage);

                if (ms.X.abs < leftBorder) {

                    rtsCameraMode.GoLeft((leftBorder - ms.X.abs) / leftBorder); // the closer to the border, the faster is the movement

                } else if (ms.X.abs > rightBorder) {
                    rtsCameraMode.GoRight((ms.X.abs - rightBorder) / leftBorder); // the closer to the border, the faster is the movement
                }

                if (ms.Y.abs < topBorder) {

                    rtsCameraMode.GoUp((topBorder - ms.Y.abs) / topBorder); // the closer to the border, the faster is the movement

                } else if (ms.Y.abs > bottomBorder) {
                    rtsCameraMode.GoDown((ms.Y.abs - bottomBorder) / topBorder); // the closer to the border, the faster is the movement
                }

                if (ms.Z.rel != 0) {
                    rtsCameraMode.ZoomIn(Mogre.Math.Sign(ms.Z.rel) * 2);

                }
            }
        }

        bool FrameStarted(FrameEvent evt)
        {
            inputKeyboard.Capture();
            inputMouse.Capture();
            return true;
        }

        public bool MouseMotion(MOIS.MouseEvent e)
        {
            if (mCameraCS != null) {
                var ms = e.state;

                var mode = mCameraCS.CurrentCameraMode;
                var name = mCameraCS.GetCameraModeName(mode);
                if (name == "Orbital + mouse") {
                    (mode as OrbitalWithMouseCameraMode).InjectMouseMoved(e);
                }
            }
            return true;
        }

        private static int i = 0;
        List<string> cameraModeNames = new List<string>();
        public bool MousePressed(MOIS.MouseEvent e, MOIS.MouseButtonID button)
        {
            if (button == MouseButtonID.MB_Right) {
                if (mCameraCS != null) {
                    i++;
                    mCameraCS.CurrentCameraMode =
                        mCameraCS.GetNextCameraMode(cameraModeNames[i % cameraModeNames.Count]);
                }
            }
            return true;
        }
        public bool MouseReleased(MOIS.MouseEvent e, MOIS.MouseButtonID button)
        {
            return true;
        }

        void processPlayerKeyInput(FrameEvent evt)
        {

            float movingFactor = 300.0f;
            float rotatingFactor = 2.0f;


            if (inputKeyboard.IsKeyDown(KeyCode.KC_I)) {
                headNode.Translate(evt.timeSinceLastFrame * movingFactor * Vector3.UNIT_Z, Mogre.Node.TransformSpace.TS_LOCAL);
            }

            if (inputKeyboard.IsKeyDown(KeyCode.KC_K)) {
                headNode.Translate(evt.timeSinceLastFrame * movingFactor * Vector3.NEGATIVE_UNIT_Z, Mogre.Node.TransformSpace.TS_LOCAL);
            }

            if (inputKeyboard.IsKeyDown(KeyCode.KC_L)) {
                headNode.Yaw(new Radian(evt.timeSinceLastFrame * -rotatingFactor), Mogre.Node.TransformSpace.TS_LOCAL);
            }

            if (inputKeyboard.IsKeyDown(KeyCode.KC_J)) {
                headNode.Yaw(new Radian(evt.timeSinceLastFrame * rotatingFactor), Mogre.Node.TransformSpace.TS_LOCAL);
            }

        }

        private void ProcessCameraInput()
        {

            if (mCameraCS.GetCameraModeName(mCameraCS.CurrentCameraMode) == "Free") {
                FreeCameraMode freeCameraMode = (FreeCameraMode)mCameraCS.GetCameraMode("Free");


                if (inputKeyboard.IsKeyDown(KeyCode.KC_A))
                    //mTranslateVector.x = -mMoveScale;	// Move camera left
                    freeCameraMode.GoLeft();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_D))
                    //mTranslateVector.x = mMoveScale;	// Move camera RIGHT
                    freeCameraMode.GoRight();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_W))
                    //mTranslateVector.z = -mMoveScale;	// Move camera forward
                    freeCameraMode.GoForward();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_S))
                    //mTranslateVector.z = mMoveScale;	// Move camera backward
                    freeCameraMode.GoBackward();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_PGUP))
                    //mTranslateVector.y = mMoveScale;	// Move camera up
                    freeCameraMode.GoUp();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_PGDOWN))
                    //mTranslateVector.y = -mMoveScale;	// Move camera down
                    freeCameraMode.GoDown();

                if (inputKeyboard.IsKeyDown(KeyCode.KC_RIGHT))
                    //mCamera->yaw(-mRotScale);
                    freeCameraMode.DoYaw(-1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_LEFT))
                    //mCamera->yaw(mRotScale);
                    freeCameraMode.DoYaw(1);
            } else if (mCameraCS.GetCameraModeName(mCameraCS.CurrentCameraMode) == "Orbital"
                       || mCameraCS.GetCameraModeName(mCameraCS.CurrentCameraMode) == "Orbital (collidable)") {
                OrbitalCameraMode orbitalCameraMode;

                if (mCameraCS.GetCameraModeName(mCameraCS.CurrentCameraMode) == "Orbital") {
                    orbitalCameraMode = (OrbitalCameraMode)mCameraCS.GetCameraMode("Orbital");
                } else {
                    orbitalCameraMode = (OrbitalCameraMode)mCameraCS.GetCameraMode("Orbital (collidable)");
                }

                if (inputKeyboard.IsKeyDown(KeyCode.KC_A))
                    orbitalCameraMode.DoYaw(1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_D))
                    orbitalCameraMode.DoYaw(-1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_W))
                    orbitalCameraMode.DoPitch(1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_S))
                    orbitalCameraMode.DoPitch(-1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_PGDOWN))
                    orbitalCameraMode.DoZoom(-1);

                if (inputKeyboard.IsKeyDown(KeyCode.KC_PGUP))
                    orbitalCameraMode.DoZoom(1);
            }
        }


        protected override void CreateScene()
        {
            // Set ambient light
            sceneMgr.AmbientLight = new ColourValue(0.75f, 0.75f, 0.75f);

            // Create a light
            Light l = sceneMgr.CreateLight("MainLight");
            // Accept default settings: point light, white diffuse, just set position
            // NB I could attach the light to a SceneNode if I wanted it to move automatically with
            //  other objects, but I don't
            l.Position = new Vector3(200, 700, 100);

            sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_MODULATIVE;

            // Create a skydome
            sceneMgr.SetSkyDome(true, "Examples/CloudySky", 30, 5);

            // Put in a bit of fog for the hell of it
            sceneMgr.SetFog(FogMode.FOG_EXP, ColourValue.White, 0.0001f, 0.5f);

            // Define a floor plane mesh
            Plane p = new Plane();
            p.normal = Vector3.UNIT_Y;
            p.d = 180;
            MeshManager.Singleton.CreatePlane("FloorPlane",
                ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME,
                p, PLANE_SIZE * 1000, PLANE_SIZE * 1000, 20, 20, true, 1, 50, 50, Vector3.UNIT_Z);

            Entity ent = sceneMgr.CreateEntity("floorEntity", "FloorPlane");
            ent.SetMaterialName("Examples/RustySteel");
            ent.CastShadows = false;
            SceneNode floorNode = sceneMgr.RootSceneNode.CreateChildSceneNode("floorSceneNode");
            floorNode.AttachObject(ent);

            // Add a head, give it it's own node
            // The Ogre head faces to Z
            headNode = sceneMgr.RootSceneNode.CreateChildSceneNode("headSceneNode");
            ent = sceneMgr.CreateEntity("head", "ogrehead.mesh");
            ent.CastShadows = true;
            headNode.AttachObject(ent);

            atheneNode = sceneMgr.RootSceneNode.CreateChildSceneNode("atheneSceneNode");
            //Entity *Athene = mSceneMgr->createEntity( "Razor", "razor.mesh" );
            Entity Athene = sceneMgr.CreateEntity("Athene", "athene.mesh");
            Athene.SetMaterialName("Examples/Athene/NormalMapped");
            Athene.CastShadows = true;
            atheneNode.AttachObject(Athene);
            atheneNode.SetPosition(500, -100, 500);

            // Obstacle for collisions detection
            SceneNode barrelNode = sceneMgr.RootSceneNode.CreateChildSceneNode("barrelSceneNode");
            Entity barrel = sceneMgr.CreateEntity("barrel", "barrel.mesh");
            barrel.CastShadows = true;
            barrelNode.AttachObject(barrel);
            barrelNode.SetPosition(1300, -100, 500);
            barrelNode.SetScale(40, 40, 40);

            // Create light node
            SceneNode lightNode = sceneMgr.RootSceneNode.CreateChildSceneNode("lightSceneNode");
            lightNode.AttachObject(l);
            goto cameraControl;

            // set up spline animation of node
            Animation anim = sceneMgr.CreateAnimation("HeadTrack", 20);
            // Spline it for nice curves

            anim.SetInterpolationMode(Mogre.Animation.InterpolationMode.IM_SPLINE);
            // Create a track to animate the camera's node
            NodeAnimationTrack track = anim.CreateNodeTrack(0, headNode);
            // Setup keyframes
            TransformKeyFrame key = track.CreateNodeKeyFrame(0); // startposition
            key.Translate = new Vector3(0, 0, 0);
            key.Rotation = Quaternion.IDENTITY;

            key = track.CreateNodeKeyFrame(2.5f);
            key.Translate = new Vector3(0, 0, 1000);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(1000, 0, 1000));

            key = track.CreateNodeKeyFrame(5);
            key.Translate = new Vector3(1000, 0, 1000);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(1000, 0, 0));

            key = track.CreateNodeKeyFrame(7.5f);
            key.Translate = new Vector3(1000, 0, 0);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(Vector3.NEGATIVE_UNIT_X);

            key = track.CreateNodeKeyFrame(10);
            key.Translate = new Vector3(0, 0, 0);

            // Second round
            key = track.CreateNodeKeyFrame(11);
            key.Translate = new Vector3(0, 0, 400);
            key.Rotation = new Quaternion(new Radian(3.14f / 4.0f), Vector3.UNIT_Z);

            key = track.CreateNodeKeyFrame(11.5f);
            key.Translate = new Vector3(0, 0, 600);
            key.Rotation = new Quaternion(new Radian(-3.14f / 4.0f), Vector3.UNIT_Z);

            key = track.CreateNodeKeyFrame(12.5f);
            key.Translate = new Vector3(0, 0, 1000);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(500, 500, 1000));

            key = track.CreateNodeKeyFrame(13.25f);
            key.Translate = new Vector3(500, 500, 1000);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(1000, -500, 1000));

            key = track.CreateNodeKeyFrame(15);
            key.Translate = new Vector3(1000, 0, 1000);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(1000, 0, -500));

            key = track.CreateNodeKeyFrame(16);
            key.Translate = new Vector3(1000, 0, 500);

            key = track.CreateNodeKeyFrame(16.5f);
            key.Translate = new Vector3(1000, 0, 600);

            key = track.CreateNodeKeyFrame(17.5f);
            key.Translate = new Vector3(1000, 0, 0);
            key.Rotation = Vector3.UNIT_Z.GetRotationTo(new Vector3(-500, 500, 0));

            key = track.CreateNodeKeyFrame(118.25f);
            key.Translate = new Vector3(500, 500, 0);
            key.Rotation = new Quaternion(new Radian(3.14f), Vector3.UNIT_X) * Vector3.UNIT_Z.GetRotationTo(new Vector3(-500, -500, 0));

            key = track.CreateNodeKeyFrame(20);
            key.Translate = new Vector3(0, 0, 0);

            key = track.CreateNodeKeyFrame(2000);
            key.Translate = new Vector3(-20000000, 0, 0);

            // Create a new animation state to track this
            mAnimState = sceneMgr.CreateAnimationState("HeadTrack");
            mAnimState.Enabled = true;

        cameraControl: ;
            setupCameraControlSystem();

        }

        private void setupCameraControlSystem()
        {
            // Ogre::camera points to -Z by default

            // Create the camera system using the previously created ogre camera.
            mCameraCS = new CameraControlSystem(sceneMgr, "CameraControlSystem", camera);

            // -------------------------------------------------------------------------------------
            // Register a "Fixed" camera mode. In this mode the camera position and orientation
            // never change.

            FixedCameraMode camMode1;
            camMode1 = new FixedCameraMode(mCameraCS, Vector3.UNIT_Y);
            //mCameraCS->registerCameraMode("Fixed (2)",camMode1);
            mCameraCS.RegisterCameraMode("Fixed", camMode1);
            cameraModeNames.Add("Fixed");
            camMode1.CameraPosition=new Vector3(-500, 0, -500);
            float roll = 0; float yaw = 225; float pitch = 10;
            camMode1.CameraOrientation=new Quaternion(new Radian(new Degree(roll)), Vector3.UNIT_Z)
                * new Quaternion(new Radian(new Degree(yaw)), Vector3.UNIT_Y)
                * new Quaternion(new Radian(new Degree(pitch)), Vector3.UNIT_X);

            // -------------------------------------------------------------------------------------
            // Register a "FixedTracking" camera mode. In this mode the camera position is fixed
            // and the camera always points to the target.

            FixedTrackingCameraMode camMode2;
            camMode2 = new FixedTrackingCameraMode(mCameraCS, Vector3.UNIT_Y);
            mCameraCS.RegisterCameraMode("FixedTracking", camMode2);
            cameraModeNames.Add("FixedTracking");
            camMode2.CameraPosition=new Vector3(500, 0, -100);

            // -------------------------------------------------------------------------------------
            // Register a "Chase" camera mode with default tightness (0.01). In
            // this mode the camera follows the target. The second parameter is the relative position
            // to the target. The orientation of the camera is fixed by a yaw axis (UNIT_Y by default).

            ChaseCameraMode camMode3;
            camMode3 = new ChaseCameraMode(mCameraCS, new Vector3(0, 0, -200), Vector3.UNIT_Y);
            //mCameraCS.registerCameraMode("Chase(0.01 tightness)",camMode3);
            mCameraCS.RegisterCameraMode("Chase", camMode3);
            cameraModeNames.Add("Chase");

            // -------------------------------------------------------------------------------------
            // Register a "ChaseFreeYawAxis" camera mode with max tightness. This mode is
            // similar to "Chase" camera mode but the camera orientation is not fixed by
            // a yaw axis. The camera orientation will be the same as the target.

            camMode3 = new ChaseFreeYawAxisCameraMode(mCameraCS, new Vector3(0, 0, -200)
                , new Radian(0), new Radian(new Degree(180)), new Radian(0));
            mCameraCS.RegisterCameraMode("ChaseFreeYawAxis", camMode3);
            cameraModeNames.Add("ChaseFreeYawAxis");
            camMode3.CameraTightness = 0.05f;

            // -------------------------------------------------------------------------------------
            // Register a "FirstPerson" camera mode.

            //FirstPersonCameraMode* camMode4 = new FirstPersonCameraMode(mCameraCS,Vector3(0,17,-16)
            FirstPersonCameraMode camMode4 = new FirstPersonCameraMode(mCameraCS, new Vector3(0, 6, -20)
                , new Radian(0), new Radian(new Degree(180)), new Radian(0));
            mCameraCS.RegisterCameraMode("FirstPerson", camMode4);
            cameraModeNames.Add("FirstPerson");
            camMode4.IsCharacterVisible = false;

            // -------------------------------------------------------------------------------------
            // Register a "PlaneBinded" camera mode. In this mode the camera is constrained to the
            // limits of a plane. The camera always points to the target, perpendicularly to the plane.

            Plane mPlane = new Plane(Vector3.UNIT_Z, new Vector3(0, 0, -200));
            PlaneBindedCameraMode camMode5 = new PlaneBindedCameraMode(mCameraCS, mPlane, Vector3.UNIT_Y);
            mCameraCS.RegisterCameraMode("PlaneBinded (XY)", camMode5);
            cameraModeNames.Add("PlaneBinded (XY)");

            // -------------------------------------------------------------------------------------
            // Register another "PlaneBinded" camera mode using a top point of view.

            //            mPlane = new Plane(Vector3.UNIT_Y, new Vector3(0, 1000, 0));
            //            camMode5 = new PlaneBindedCameraMode(mCameraCS, mPlane, Vector3.UNIT_Z);
            //            mCameraCS.registerCameraMode("PlaneBinded (XZ)", camMode5);
            //            cameraModeNames.Add("PlaneBinded (XZ)");

            // -------------------------------------------------------------------------------------
            // Register a "ThroughTarget" camera mode. In this mode the camera points to a given
            // position (the "focus") throuh the target. The camera orientation is fixed by a yaw axis.

            ThroughTargetCameraMode camMode6 = new ThroughTargetCameraMode(mCameraCS, 400, Vector3.ZERO, Vector3.UNIT_Y);
            mCameraCS.RegisterCameraMode("ThroughTarget", camMode6);
            cameraModeNames.Add("ThroughTarget");
            //camMode6.CameraFocusPosition=atheneNode._getDerivedPosition() - Vector3(0,100,0);
            camMode6.CameraFocusPosition=atheneNode._getDerivedPosition() + new Vector3(0, 100, 0);

            // -------------------------------------------------------------------------------------
            // Register a "ClosestToTarget" camera mode. In this camera mode the position of the
            // camera is chosen to be the closest to the target of a given list. The camera
            // orientation is fixed by a yaw axis.

            ClosestToTargetCameraMode camMode7 = new ClosestToTargetCameraMode(mCameraCS, Vector3.UNIT_Y);
            mCameraCS.RegisterCameraMode("ClosestToTarget", camMode7);
            cameraModeNames.Add("ClosestToTarget");

            Vector3 camPos1 = new Vector3(-400, 0, -400);
            Vector3 camPos2 = new Vector3(-400, 0, 1400);
            Vector3 camPos3 = new Vector3(1400, 0, 1400);

            camMode7.AddCameraPosition(camPos1);
            camMode7.AddCameraPosition(camPos2);
            camMode7.AddCameraPosition(camPos3);

            // -------------------------------------------------------------------------------------
            // Register an "Attached" camera mode. In this mode the camera node is attached to the
            // target node as a child.

            AttachedCameraMode camMode8 = new AttachedCameraMode(mCameraCS, new Vector3(200, 0, 0)
                , new Radian(0), new Radian(new Degree(90)), new Radian(0));
            //mCameraCS.registerCameraMode("Attached (lateral)",camMode8);
            mCameraCS.RegisterCameraMode("Attached", camMode8);
            cameraModeNames.Add("Attached");

            // -------------------------------------------------------------------------------------
            // Register a "Free" camera mode. In this mode the camera is controlled by the user.
            // The camera orientation is fixed to a yaw axis.

            yaw = 225; pitch = -10;
            FreeCameraMode camMode9 = new FreeCameraMode(mCameraCS, Vector3.ZERO, new Degree(yaw), new Degree(pitch)
                , SwitchingMode.CurrentState);
            mCameraCS.RegisterCameraMode("Free", camMode9);
            cameraModeNames.Add("Free");
            camMode9.MoveFactor=30;

            // -------------------------------------------------------------------------------------
            // Register a "FixedDirection" camera mode. In this mode the
            // target is always seen from the same point of view.

            FixedDirectionCameraMode camMode10 = new FixedDirectionCameraMode(mCameraCS, new Vector3(-1, -1, -1), 1000, Vector3.UNIT_Y);
            mCameraCS.RegisterCameraMode("Fixed direction", camMode10);
            cameraModeNames.Add("Fixed direction");
            camMode10.CameraTightness=0.01f;

            // -------------------------------------------------------------------------------------
            // Register an "Orbital" camera mode. This is basically an attached camera mode where the user
            // can mofify the camera position. If the scene focus is seen as the center of a sphere, the camera rotates arount it.
            // The last parameter indicates if the camera should be reset to its initial position when this mode is selected.

            OrbitalCameraMode camMode12 = new OrbitalCameraMode(mCameraCS, new Radian(0), new Radian(0), 200, false);
            mCameraCS.RegisterCameraMode("Orbital", camMode12);
            cameraModeNames.Add("Orbital");
            camMode12.ZoomFactor=100;
            camMode12.RotationFactor=50;
            //camMode12.CollisionsEnabled = true;
            //camMode12.collisionDelegate = camMode12.DefaultCollisionDetectionFunction;
            // ** Uncomment for custom collisions calculation. By default the collisions are based on ogre basic raycast feature **
            //camMode12.collisionDelegate = CollidableCamera::newCollisionDelegate(this
            //	, &CameraControlSystemDemo::CustomCollisionDetectionFunction);

            // -------------------------------------------------------------------------------------
            // Register a RTS camera mode.
            //

            RTSCameraMode camMode13 = new RTSCameraMode(mCameraCS,
                new Vector3(500, 1300, 1000)
                , Vector3.NEGATIVE_UNIT_Z
                , Vector3.NEGATIVE_UNIT_X
                , new Radian(new Degree(70))
                , 0, 1000);
            mCameraCS.RegisterCameraMode("RTS", camMode13);
            cameraModeNames.Add("RTS");
            camMode13.MoveFactor=20;

            // -------------------------------------------------------------------------------------
            // Register the custom "Dummy" camera mode defined previously. It basically goes forward
            // and backward constantly

            DummyCameraMode camMode14 = new DummyCameraMode(mCameraCS, 400);
            mCameraCS.RegisterCameraMode("Dummy", (CameraMode)camMode14);
            cameraModeNames.Add("Dummy");

            // -------------------------------------------------------------------------------------
            // Register an "OrbitalWithMouse" camera mode. 

            OrbitalWithMouseCameraMode camMode15 = new OrbitalWithMouseCameraMode(mCameraCS, MOIS.MouseButtonID.MB_Left, new Radian(0), new Radian(0), 200);
            mCameraCS.RegisterCameraMode("Orbital + mouse", camMode15);
            cameraModeNames.Add("Orbital + mouse");
            camMode15.ZoomFactor=3;
            camMode15.RotationFactor=10;

            // -------------------------------------------------------------------------------------
            // Register a spheric camera mode. 

            Vector3 relativePositionToCameraTarget = new Vector3(0, 50, -300);

            // THIS IS NOT NECESSARY. just needed for the demo showing integer values in the sliders
            relativePositionToCameraTarget = relativePositionToCameraTarget.NormalisedCopy * 300;

            SphericCameraMode camMode16 = new SphericCameraMode(mCameraCS, relativePositionToCameraTarget, 700, Vector3.ZERO, Vector3.UNIT_Y);
            // outer radious = inner radious = relativePositionToCameraTarget.length() for a perfect sphere
            camMode16.HeightOffset = 50;
            mCameraCS.RegisterCameraMode("Spherical", camMode16);
            cameraModeNames.Add("Spherical");

            // Set the camera target
            mCameraCS.TargetNode = headNode;

            //mCameraCS.CurrentCameraMode=camMode1;// fixed camera of cause works
            //mCameraCS.CurrentCameraMode=camMode2;//fixed tracking camera works
            //mCameraCS.CurrentCameraMode=camMode3;//chase camera works
            //mCameraCS.CurrentCameraMode=camMode4;//first person camera works
            //mCameraCS.CurrentCameraMode=camMode5;// plane binded (XY) (YZ) camera works
            //mCameraCS.CurrentCameraMode=camMode6;// through target camera works, cs game
            //mCameraCS.CurrentCameraMode=camMode7;// closest target camera works
            //mCameraCS.CurrentCameraMode=camMode8;// attached camera works
            //mCameraCS.CurrentCameraMode=camMode9;// free camera works
            //mCameraCS.CurrentCameraMode=camMode10;// fix direction camera works
            //mCameraCS.CurrentCameraMode = camMode12;// orbital camera works, but the head entity flickered
            //mCameraCS.CurrentCameraMode=camMode13;// RTS camera works?
            //mCameraCS.CurrentCameraMode=camMode14;// dummy camera works
            mCameraCS.CurrentCameraMode=camMode15;// orbital with mouse camera works
            //mCameraCS.CurrentCameraMode=camMode16;// spherical camera works
        }

        Vector3 CustomCollisionDetectionFunction(Vector3 cameraTargetPosition, Vector3 cameraPosition)
        {
            // Here you have to implement your custom collision code.
            // The returning value is the desired camera position.
            // In this method you usually have to calculate (using OgreNewt, NxOgre, OgreODE, OgreBullet... code)
            // the first intersection between the camera target and the camera (in this order).

            return cameraPosition;
        }

    }

}
