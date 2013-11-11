/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  CameraControlSystemDemo
 * history:  created by LinJiarui 2013/10/19 星期六 19:22:21
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using Mogre;

namespace Mccs
{
    public class CameraControlSystem : IDisposable
    {
        private SceneManager _sceneMgr;
        private string _name;

        private Camera _camera;// ogre camera
        private SceneNode _cameraNode;// Where the camera is attached to
        private bool _isOwnCamera;// true if the ogre camera has been created by this class

        private SceneNode _targetNode;// The scene focus
        private NodeListener _targetNodeListener; // To recalculate the camera position if the target scenenode has been changed within the current frame
        private float _timeSinceLastFrameLastUpdate;// Time value passed to the last call of the method "Update"
        private bool _recalcOnTargetMoving;

        private CameraMode _currentCameraMode;
        private Dictionary<string, CameraMode> _cameraModes; // The list of camera mode instances

        public CameraControlSystem(SceneManager sceneManager, string name, Camera camera = null, bool reCalcOnTargetMoving = true)
        {
            _sceneMgr = sceneManager;
            _name = name;
            _targetNode = null;
            _targetNodeListener = null;
            _recalcOnTargetMoving = reCalcOnTargetMoving;
            _currentCameraMode = null;

            _cameraNode = _sceneMgr.RootSceneNode.CreateChildSceneNode(_name + "SceneNode");

            if (camera == null) {
                _camera = _sceneMgr.CreateCamera(_name);
                _isOwnCamera = true;
            } else {
                _camera = camera;
                _isOwnCamera = false;
            }

            //Reset to default parameters
            _camera.Position = Vector3.ZERO;
            _camera.Orientation = Quaternion.IDENTITY;

            // ... and attach the Ogre camera to the camera node
            _cameraNode.AttachObject(_camera);

            _cameraModes = new Dictionary<string, CameraMode>();
        }

        public CameraControlSystem(SceneManager sceneManager, string name, SceneNode customCameraSceneNode, bool reCalcOnTargetMoving = true)
        {
            _sceneMgr = sceneManager;
            _name = name;
            _targetNode = null;
            _targetNodeListener = null;
            _recalcOnTargetMoving = reCalcOnTargetMoving;
            _currentCameraMode = null;

            _cameraNode = _sceneMgr.RootSceneNode.CreateChildSceneNode(_name + "SceneNode");
            _isOwnCamera = false;
            _cameraNode.AddChild(customCameraSceneNode);

            _cameraModes = new Dictionary<string, CameraMode>();
        }

        public void Dispose()
        {
            if (_targetNode != null && _targetNode.GetListener() != null) {
                //todo dispose listener
                _targetNode.SetListener(null);
            }

            _cameraNode.SetAutoTracking(false);
            _cameraNode.RemoveAllChildren();
            _cameraNode.DetachAllObjects();
            if (_isOwnCamera) {
                _sceneMgr.DestroyCamera(_camera);
            }
            _sceneMgr.DestroySceneNode(_cameraNode);
        }

        public void RegisterCameraMode(string name, CameraMode cameraMode)
        {
            _cameraModes[name] = cameraMode;
        }

        public void RemoveCameraMode(CameraMode cameraMode)
        {
            _cameraModes.Remove(GetCameraModeName(cameraMode));
        }

        public virtual void DeleteCameraModes()
        {
            foreach (var mCameraMode in _cameraModes) {
                mCameraMode.Value.Dispose();
            }
            _cameraModes.Clear();
            _currentCameraMode = null;
        }

        public CameraMode GetCameraMode(string name)
        {
            if (_cameraModes.ContainsKey(name)) {
                return _cameraModes[name];
            }
            return null;
        }

        public CameraMode GetNextCameraMode(string name)
        {
            if (_cameraModes.Count > 0 && _cameraModes.ContainsKey(name)) {
                return _cameraModes[name];
            }

            return null;
        }

        public string GetCameraModeName(CameraMode cameraMode)
        {
            foreach (var pair in _cameraModes) {
                if (pair.Value == cameraMode) {
                    return pair.Key;
                }
            }

            return "";
        }

        public CameraMode CurrentCameraMode
        {
            get { return _currentCameraMode; }
            set
            {
                if (_currentCameraMode != null) {
                    _currentCameraMode.Stop();
                }
                //todo if value is a new mode, should add to _cameraModes
                _currentCameraMode = value;
                _currentCameraMode.Init();
                _cameraNode.Position = _currentCameraMode.CameraPosition;
                _cameraNode.Orientation = _currentCameraMode.CameraOrientation;
            }
        }

        public void Update(float timeSinceLastFrame)
        {
            _timeSinceLastFrameLastUpdate = timeSinceLastFrame;

            if (_currentCameraMode != null) {
                _currentCameraMode.Update(timeSinceLastFrame);
                _cameraNode.Position = _currentCameraMode.CameraPosition;
                _cameraNode.Orientation = _currentCameraMode.CameraOrientation;
                //var local = _cameraNode.GetChild(0);
                //var pos = local._getDerivedPosition();
                //var ori = local._getDerivedOrientation();
            }
        }

        public Camera OgreCamera { get { return _camera; } set { _camera = value; } }

        public SceneNode TargetNode
        {
            get { return _targetNode; }
            set
            {
                if (_targetNode != null && _targetNode.GetListener() != null) {
                    //todo check whether need dispose here
                    //delete _targetNode->getListener();
                    _targetNode.SetListener(null);
                }

                if (_currentCameraMode != null) {
                    _currentCameraMode.Stop();
                }

                _targetNode = value;

                if (_currentCameraMode != null) {
                    _currentCameraMode.Init();
                }

                if (_recalcOnTargetMoving && _targetNode != null) {
                    _targetNodeListener = new NodeListener(this);
                    _targetNode.SetListener(_targetNodeListener);
                }
            }
        }

        public bool HasCameraTarget { get { return _targetNode != null; } }

        public bool AutoTrackingTarget
        {
            get
            {
                if (_targetNode != null) {
                    return (_cameraNode.AutoTrackTarget != null);
                } else {
                    return false;
                }
            }
            set
            {
                if (_targetNode != null) {
                    _cameraNode.SetAutoTracking(value, _targetNode);
                } else {
                    _cameraNode.SetAutoTracking(value);
                }
            }
        }

        public float TimeSinceLastFrameLastUpdate
        {
            get { return _timeSinceLastFrameLastUpdate; }
        }

        public SceneManager SceneManager { get { return _sceneMgr; } }

        public SceneNode CameraSceneNode
        {
            get { return _cameraNode; }
        }

        public Vector3 CameraTargetPosition
        {
            get
            {
                return _targetNode._getDerivedPosition();
            }
        }

        public Vector3 CameraPosition
        {
            get { return _cameraNode._getDerivedPosition(); }
        }

        public Quaternion CameraTargetOrientation
        {
            get { return _targetNode._getDerivedOrientation(); }
        }

        public Quaternion CameraOrientation
        {
            get { return _cameraNode._getDerivedOrientation(); }
        }

        public void AddToIgnoreList(MovableObject obj)
        {
            foreach (var mode in _cameraModes) {
                if (mode.Value is ICollidableCamera) {
                    (mode.Value as ICollidableCamera).AddToIgnoreList(obj);
                }
            }
        }

        public void SetFixedYawAxis(bool useFixedAxis, Vector3 fixedAxis)
        {
            _cameraNode.SetFixedYawAxis(useFixedAxis, fixedAxis);
        }

        public void _setCameraPosition(Vector3 position)
        {
            if (_cameraNode != null) {
                _cameraNode.Position = position;
            }
        }

        public void _setCameraOrientation(Quaternion orientation)
        {
            if (_cameraNode != null) {
                _cameraNode.Orientation = orientation;
            }
        }

        class NodeListener : Mogre.Node.Listener
        {
            protected CameraControlSystem cameraCS;

            public NodeListener(CameraControlSystem cam)
            {
                cameraCS = cam;
            }

            public override void NodeUpdated(Node param1)
            {
                //base.NodeUpdated(param1);
                cameraCS.Update(cameraCS.TimeSinceLastFrameLastUpdate);
            }

        }
    }

}
