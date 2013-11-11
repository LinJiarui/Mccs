/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  MyGameApp
 * history:  created by LinJiarui 2013/10/24 星期四 20:55:53
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace Samples
{
    public class MyGameApp
    {
        public void Go()
        {
            try {
                CreateRoot();

                DefineResources();

                CreateRenderSystem();

                CreateRenderWindow();

                InitializeResources();

                ChooseSceneManager();

                CreateScene();

                CreateFrameListeners();

                EnterRenderLoop();

                DestroyData();

                mRoot.Dispose();
                mRoot = null;
            } catch (OperationCanceledException e) {
                Console.WriteLine(e);
            }
        }

        protected void DestroyData()
        {

        }

        public Root mRoot;
        public Viewport mViewport;
        protected void CreateRoot()
        {
            mRoot = new Root();
            //manually load plugins as following
            //            mRoot=new Root("");//prevent ogre looking for any plugins configuration file.
            //            mRoot.LoadPlugin("RenderSystem_Direct3D");
            //            mRoot.LoadPlugin("RenderSystem_GL");
            //            mRoot.LoadPlugin("Plugin_OctreeSceneManager");


        }

        protected void DefineResources()
        {
            ConfigFile cf = new ConfigFile();
            cf.Load("resources.cfg", "\t:=", true);

            var section = cf.GetSectionIterator();
            while (section.MoveNext()) {
                foreach (var line in section.Current) {
                    ResourceGroupManager.Singleton.AddResourceLocation(line.Value, line.Key, section.CurrentKey);
                }
            }
        }

        protected void CreateRenderSystem()
        {
            if (!mRoot.ShowConfigDialog()) {
                throw new OperationCanceledException();
            }

            //manually
            //            RenderSystem renderSystem = mRoot.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
            //            renderSystem.SetConfigOption("Full Screen", "No");
            //            renderSystem.SetConfigOption("Video Mode", "800 x 600 @ 32-bit colour");
            //            mRoot.RenderSystem = renderSystem;
        }

        protected RenderWindow mRenderWindow;
        protected void CreateRenderWindow()
        {
            mRenderWindow = mRoot.Initialise(true, "Main Ogre Window");
            mViewport = mRenderWindow.GetViewport(0);
            //works with Windows.Forms
            //            mRoot.Initialise(false, "Main Ogre Window");
            //            var win = new System.Windows.Forms.Form();
            //            NameValuePairList misc = new NameValuePairList();
            //            misc["externalWindowHandle"] = win.Handle.ToString();
            //            mRenderWindow = mRoot.CreateRenderWindow("Main RenderWindow", 800, 600, false, misc);
        }

        protected void InitializeResources()
        {
            TextureManager.Singleton.DefaultNumMipmaps = 5;
            ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
        }

        public Camera camera;
        public SceneManager sceneMgr;

        protected virtual void ChooseSceneManager()
        {
            sceneMgr = mRoot.CreateSceneManager(SceneType.ST_GENERIC);
            camera = sceneMgr.CreateCamera("Camera");
            camera.Position = new Vector3(0, 0, 150);
            camera.LookAt(Vector3.ZERO);
            mViewport = mRenderWindow.AddViewport(camera);
        }

        protected virtual void CreateScene()
        {
            camera = sceneMgr.CreateCamera("Camera");
            camera.Position = new Vector3(0, 0, 150);
            camera.LookAt(Vector3.ZERO);
            mViewport = mRenderWindow.AddViewport(camera);

            Entity ogreHead = sceneMgr.CreateEntity("Head", "ogrehead.mesh");
            SceneNode headNode = sceneMgr.RootSceneNode.CreateChildSceneNode();
            headNode.AttachObject(ogreHead);

            sceneMgr.AmbientLight = new ColourValue(0.5f, 0.5f, 0.5f);

            Light l = sceneMgr.CreateLight("MainLight");
            l.Position = new Vector3(20, 80, 50);
        }

        protected float mTimer = 5;
        protected virtual void CreateFrameListeners()
        {
            mRoot.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(OnFrameRenderingQueued);
        }

        private bool OnFrameRenderingQueued(FrameEvent evt)
        {
            return true;
            mTimer -= evt.timeSinceLastFrame;
            return (mTimer > 0);
        }

        protected void EnterRenderLoop()
        {
            mRoot.StartRendering();
        }
    }
}
