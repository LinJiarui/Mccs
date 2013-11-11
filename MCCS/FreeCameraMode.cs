/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  FreeCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 14:41:58
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera is controlled by the user. 
    /// The camera orientation is fixed by a yaw axis.
    /// </summary>
    public class FreeCameraMode : CameraMode, ICollidableCamera
    {
        private Vector3 _fixedAxis;
        private float _moveFactor;
        private float _rotationFactor;
        private float _longitudinalDisplacement;
        private float _lateralDisplacement;
        private float _verticalDisplacement;
        private Radian _rotX;
        private Radian _rotY;
        private Degree _initialRotationX;
        private Degree _initialRotationY;
        private Vector3 _initialPosition;
        private Degree _lastRotationX;
        private Degree _lastRotationY;
        private Vector3 _lastPosition;
        private SwitchingMode _switchingMode;
        private List<MovableObject> _ignoreList;
        /**
         * @param margin Collision margin
         * @param switchingMode Determine the state of the camera when switching to this camera mode from another
         */
        public FreeCameraMode(CameraControlSystem cam
              , Vector3 initialPosition
              , Degree initialRotationX
              , Degree initialRotationY
              , SwitchingMode switchingMode = SwitchingMode.CurrentState
              , float margin = 0.1f)
            : base(cam)
        {
            CameraCS = cam;
            Margin = margin;

            _fixedAxis = Vector3.UNIT_Y;
            _moveFactor = 1;
            _rotationFactor = 0.13f;
            _rotX = initialRotationX;
            _rotY = initialRotationY;
            _initialPosition = initialPosition;
            _initialRotationX = initialRotationX;
            _initialRotationY = initialRotationY;
            _lastRotationX = initialRotationX;
            _lastRotationY = initialRotationY;
            _lastPosition = initialPosition;
            _switchingMode = switchingMode;

            CameraPosition = initialPosition;

            this.CollisionFunc = this.DefaultCollisionDetectionFunction;
        }


        #region Implementation of ICollidableCamera

        public float Margin
        {
            get;
            set;
        }

        public bool CollisionsEnabled
        {
            get;
            set;
        }

        public CameraControlSystem CameraCS
        {
            get;
            set;
        }

        public List<MovableObject> IgnoreList
        {
            get { return _ignoreList; }
        }

        public void AddToIgnoreList(MovableObject obj)
        {
            _ignoreList.Add(obj);
        }

        public Func<Vector3, Vector3, Vector3> CollisionFunc
        {
            get;
            set;
        }

        #endregion


        public override bool Init()
        {
            //CameraMode::Init();
            base.Init();

            base.CameraCS.SetFixedYawAxis(true, _fixedAxis);

            _longitudinalDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;

            if (_switchingMode == SwitchingMode.CurrentState) {
                CameraPosition = base.CameraCS.OgreCamera.RealPosition;

                Vector3 cameraDirection = base.CameraCS.OgreCamera.RealDirection;
                if (base.CameraCS.AutoTrackingTarget) {
                    cameraDirection = base.CameraCS.CameraTargetPosition - CameraPosition;
                }
                cameraDirection.Normalise();

                Vector2 yawandpitch = GetYawAndPitch(cameraDirection);
                _rotX = yawandpitch.x - Mogre.Math.HALF_PI;
                _rotY = yawandpitch.y;
            } else if (_switchingMode == SwitchingMode.LastState) {
                _rotX = _lastRotationX;
                _rotY = _lastRotationY;
                CameraPosition = _lastPosition;
            } else {
                Reset();
            }

            base.CameraCS.AutoTrackingTarget = false;

            InstantUpdate();

            return true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException("camera mode: " + this.GetType().Name);
        }

        public override void Update(float timeSinceLastFrame)
        {
            Vector3 dirVector = base.CameraCS.OgreCamera.RealDirection;
            Vector3 lateralVector = dirVector.CrossProduct(_fixedAxis).NormalisedCopy;
            Vector3 upVector = -dirVector.CrossProduct(lateralVector).NormalisedCopy;

            Vector3 displacement = ((dirVector * _longitudinalDisplacement)
                + (upVector * _verticalDisplacement)
                + (lateralVector * _lateralDisplacement)) * timeSinceLastFrame * _moveFactor;

            if (CollisionsEnabled) {
                CameraPosition = CollisionFunc(CameraPosition, CameraPosition + displacement);
            } else {
                CameraPosition += displacement;
            }

            Quaternion offsetX = new Quaternion(_rotY, Vector3.UNIT_X);
            Quaternion offsetY = new Quaternion(_rotX, _fixedAxis);

            CameraOrientation = offsetY * offsetX;

            _longitudinalDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;

            _lastRotationX = _rotX;
            _lastRotationY = _rotY;
            _lastPosition = CameraPosition;
        }

        public override void InstantUpdate()
        {
            Quaternion offsetX = new Quaternion(_rotY, Vector3.UNIT_X);
            Quaternion offsetY = new Quaternion(_rotX, _fixedAxis);

            CameraOrientation = offsetY * offsetX;

            _longitudinalDisplacement = 0;
            _lateralDisplacement = 0;
            _verticalDisplacement = 0;

            _lastRotationX = _rotX;
            _lastRotationY = _rotY;
            _lastPosition = CameraPosition;
        }

        public Vector3 CameraPosition { get; set; }
        public Quaternion CameraOrientation { get; set; }

        /// <summary>
        /// the units the camera will be translated per second
        /// </summary>
        public float MoveFactor { get { return _moveFactor; } set { _moveFactor = value; }}

        /// <summary>
        /// the radians the camera will be rotated per second
        /// </summary>
        public float RotationFactor { get { return _rotationFactor; }set { _rotationFactor = value; } }

        /// <summary>
        /// InitialState for resetting the camera to its initial state; CurrentState for mantaining the current camera position and orientation; LastState for returning to the last known camera state when this camera mode was actived.
        /// </summary>
        public SwitchingMode SwitchingMode { get { return _switchingMode; }set { _switchingMode = value; } }

        /// <summary>
        /// the amount of rotation (use negative values to look left)
        /// </summary>
        public float Yaw { get { return -_rotX.ValueDegrees; } set { _rotX = new Degree(-value); } }

        /// <summary>
        /// the amount of rotation (use negative values to look up)
        /// </summary>
        public float Pitch { get { return -_rotY.ValueDegrees; } set { _rotY = new Degree(-value); } }

        /// <summary>
        /// Tell the camera to go forward
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoForward(float percentage = 1) { _longitudinalDisplacement += _moveFactor * percentage; }
        
        /// <summary>
        /// Tell the camera to go backward
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoBackward(float percentage = 1) { GoForward(-percentage); }

        /// <summary>
        /// Tell the camera to go right (laterally)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoRight(float percentage = 1) { _lateralDisplacement += _moveFactor * percentage; }

        /// <summary>
        /// Tell the camera to go left (laterally)
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoLeft(float percentage = 1) { GoRight(-percentage); }

        /// <summary>
        /// Tell the camera to go up
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoUp(float percentage = 1) { _verticalDisplacement += _moveFactor * percentage; }

        /// <summary>
        /// Tell the camera to go down
        /// </summary>
        /// <param name="percentage">percentage the relative speed of the movement acording to the move factor (1: 100%, 0: 0%, -1: -100%)</param>
        public void GoDown(float percentage = 1) { GoUp(-percentage); }

        /// <summary>
        /// Tell the camera to look right
        /// </summary>
        /// <param name="val">val the amount of rotation (use negative values to look left)</param>
        public void DoYaw(float val) { _rotX += new Degree(-val * _rotationFactor); }

        /// <summary>
        /// Tell the camera to look down
        /// </summary>
        /// <param name="val">val the amount of rotation (use negative values to look up)</param>
        public void DoPitch(float val) { _rotY += new Degree(-val * _rotationFactor); }

        public void Reset()
        {
            CameraPosition = _initialPosition;
            _rotX = _initialRotationX;
            _rotY = _initialRotationY;
        }

        

        /// <summary>
        /// Collision delegate that mantains the camera always above the ground.
        ///  A margin can be stablished using the 'Margin' property
        /// </summary>
        /// <param name="cameraTargetPosition"></param>
        /// <param name="cameraPosition"></param>
        /// <returns></returns>
        public Vector3 AboveGroundCollisionDetectionFunction(Vector3 cameraTargetPosition, Vector3 cameraPosition)
        {
            Vector3 origin = cameraPosition + (_fixedAxis * 100000);
            Vector3 direction = -_fixedAxis;

            Vector3 projectedCameraPosition = GetFirstRealHit(origin, direction);
            projectedCameraPosition.y += Margin;
            if (projectedCameraPosition.y < cameraPosition.y) {
                return cameraPosition;
            } else {
                return projectedCameraPosition;
            }
        }

        /// <summary>
        /// Collision delegate that mantains a constant distance to the ground.
        ///  The distance can be stablished using the 'Margin' property.
        /// </summary>
        /// <param name="cameraTargetPosition"></param>
        /// <param name="cameraPosition"></param>
        /// <returns></returns>
        public Vector3 ConstantHeightCollisionDetectionFunction(Vector3 cameraTargetPosition, Vector3 cameraPosition)
        {
            Vector3 origin = cameraPosition + (_fixedAxis * 100000);
            Vector3 direction = -_fixedAxis;

            Vector3 projectedCameraPosition = GetFirstRealHit(origin, direction);

            return projectedCameraPosition + (_fixedAxis * Margin);
        }


        protected Vector3 GetFirstRealHit(Vector3 origin, Vector3 direction)
        {
            Vector3 hit = origin;
            float minDistance = float.MaxValue;

            RaySceneQuery raySceneQuery = CameraCS.SceneManager.CreateRayQuery(new Ray(origin, direction));

            RaySceneQueryResult result = raySceneQuery.Execute();
            var iterator = result.GetEnumerator();

            bool intersect = false;
            while (iterator.MoveNext() /*&& !intersect*/)
            //! @todo are the results ordered ?? if so, uncomment (optimization)

    {
                var itr = iterator.Current;
                if (itr.distance < minDistance // take the shorter
                    && itr.movable.ParentSceneNode != CameraCS.CameraSceneNode
                    && itr.movable.ParentSceneNode != CameraCS.TargetNode
                    && !_ignoreList.Contains(itr.movable)) {
                    minDistance = itr.distance;
                    intersect = true;
                    if (itr.worldFragment != null) {
                        hit = itr.worldFragment.singleIntersection;
                    } else //if(itr.movable)
			{
                        hit = origin + (direction * itr.distance);
                    }
                }

            }

            CameraCS.SceneManager.DestroyQuery(raySceneQuery);

            return hit;
        }

        protected Vector2 GetYawAndPitch(Vector3 direction)
        {
            // Adapted from http://www.ogre3d.org/forums/viewtopic.php?f=2&t=27086

            Vector2 yawandpitch = new Vector2(0, 0); // yawandpitch.x = yaw, yawandpitch.y = pitch

            if (direction.x == 0.0f && direction.z == 0.0f) {
                yawandpitch.x = 0.0f;

                if (direction.y > 0.0f) {
                    yawandpitch.y = 1.5f * Mogre.Math.PI;
                } else {
                    yawandpitch.y = Mogre.Math.HALF_PI;
                }
            } else {
                yawandpitch.x = Mogre.Math.ATan2(-direction.z, direction.x).ValueRadians;

                if (yawandpitch.x < 0.0f) {
                    yawandpitch.x += 2.0f * Mogre.Math.PI;
                }

                var forward = Mogre.Math.Sqrt(direction.x * direction.x + direction.z * direction.z);

                yawandpitch.y = Mogre.Math.ATan2(direction.y, forward).ValueRadians;

                if (yawandpitch.y < 0.0f) {
                    yawandpitch.y += 2.0f * Mogre.Math.PI;
                }
            }

            return yawandpitch;
        }

    }

    public enum SwitchingMode { InitialState = 0, CurrentState, LastState };

}
