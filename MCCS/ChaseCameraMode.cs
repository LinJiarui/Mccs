/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  ChaseCameraMode
 * history:  created by LinJiarui 2013/10/20 星期日 9:09:01
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using Mogre;

namespace Mccs
{
    /// <summary>
    /// In this mode the camera follows the target. The second parameter is the relative position
    /// to the target. The orientation of the camera is fixed by a yaw axis (UNIT_Y by default). 
    /// </summary>
    public class ChaseCameraMode : CameraModeWithTightness, ICollidableCamera
    {
        private List<MovableObject> _ignoreList;
        private Vector3 _fixedAxis;
        private Vector3 _relativePositionToCameraTarget;
        private bool _fixedStep;
        private float _delta;
        private float _remainingTime;

        public ChaseCameraMode(CameraControlSystem cam, Vector3 relativePositionToCameraTarget, Vector3 fixedAxis, float margin = 0.1f, bool fixedStep = false, float delta = 0.001f)
            : base(cam)
        {
            Margin = margin;
            CameraCS = cam;

            _fixedAxis = fixedAxis;
            _relativePositionToCameraTarget = relativePositionToCameraTarget;
            _fixedStep = fixedStep;
            _delta = delta;

            CameraTightness = 0.01f;
            _remainingTime = 0;
        }

        public override bool Init()
        {
            //todo Init() should use CameraMode.Init()
            //CameraMode.Init();
            CameraPosition = ((CameraMode)this).CameraCS.CameraPosition;
            CameraOrientation = ((CameraMode)this).CameraCS.CameraOrientation;

            _delta = 0;
            _remainingTime = 0;

            SetFixedYawAxis(true, _fixedAxis);
            ((CameraMode) this).CameraCS.AutoTrackingTarget = true;

            this.CollisionFunc = this.DefaultCollisionDetectionFunction;

            InstantUpdate();

            return true;
        }

        public override void Update(float timeSinceLastFrame)
        {
            if (((CameraMode) this).CameraCS.HasCameraTarget) {
                var cameraCurrentPosition = ((CameraMode) this).CameraCS.CameraPosition;
                var cameraFinalPositionIfNoTightness = ((CameraMode) this).CameraCS.CameraTargetPosition
                        + ((CameraMode) this).CameraCS.CameraTargetOrientation * _relativePositionToCameraTarget;

                if (!_fixedStep) {
                    var diff = (cameraFinalPositionIfNoTightness - cameraCurrentPosition) * CameraTightness;
                    CameraPosition += diff;
                    //! @todo CameraPos += diff * timeSinceLastFrame; this makes the camera mode time independent but it also make impossible to get a completely rigid link (tightness = 1)
                } else {
                    _remainingTime += timeSinceLastFrame;
                    int steps = (int)(_remainingTime / _delta);
                    var mFinalTime = steps * _delta;
                    var cameraCurrentOrientation = ((CameraMode) this).CameraCS.CameraOrientation;
                    var finalPercentage = mFinalTime / _remainingTime;
                    var cameraFinalPosition = cameraCurrentPosition + ((cameraFinalPositionIfNoTightness - cameraCurrentPosition) * finalPercentage);
                    var cameraFinalOrientation = Quaternion.Slerp(finalPercentage, cameraCurrentOrientation
                                                                                        , ((CameraMode) this).CameraCS.CameraTargetOrientation);

                    var cameraIntermediatePosition = cameraCurrentPosition;
                    var cameraIntermediateOrientation = cameraCurrentOrientation;
                    for (int i = 0; i < steps; i++) {
                        var percentage = ((i + 1) / (float)steps);

                        var intermediatePositionIfNoTightness = cameraCurrentPosition + ((cameraFinalPositionIfNoTightness - cameraCurrentPosition) * percentage);

                        var diff = (intermediatePositionIfNoTightness - cameraCurrentPosition) * CameraTightness;
                        CameraPosition += diff;
                    }
                }

                if (CollisionsEnabled) {
                    CameraPosition = CollisionFunc(((CameraMode)this).CameraCS.CameraTargetPosition, CameraPosition);
                }
            }
        }

        public override void InstantUpdate()
        {
            if ( ((CameraMode) this).CameraCS.HasCameraTarget) {
                CameraPosition = ((CameraMode)this).CameraCS.CameraTargetPosition
                    + ((CameraMode) this).CameraCS.CameraTargetOrientation * _relativePositionToCameraTarget;

                if (CollisionsEnabled) {
                    CameraPosition = CollisionFunc(((CameraMode)this).CameraCS.CameraTargetPosition, CameraPosition);
                }
            }
        }

        public virtual void SetCameraRelativePosition(Vector3 posRelativeToCameraTarget)
        {
            _relativePositionToCameraTarget = posRelativeToCameraTarget;
            InstantUpdate();
        }

        public virtual void SetFixedYawAxis(bool useFixedAxis, Vector3 fixedAxis)
        {
            _fixedAxis = fixedAxis;
            ((CameraMode) this).CameraCS.SetFixedYawAxis(true, _fixedAxis);
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

        public Vector3 RelativePosToTarget { get { return _relativePositionToCameraTarget; }internal set { _relativePositionToCameraTarget = value; }}
        public Vector3 FixedAxis { get { return _fixedAxis; }internal set { _fixedAxis = value; }}
    }
}
