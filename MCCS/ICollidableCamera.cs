/* ***********************************************
 * author :  LinJiarui/exAt/ex@
 * file   :  ICollidableCamera
 * history:  created by LinJiarui 2013/10/19 星期六 19:45:44
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mogre;

namespace Mccs
{
    public interface ICollidableCamera
    //:CameraMode
    {
        float Margin { get; set; }
        bool CollisionsEnabled { get; set; }
        CameraControlSystem CameraCS { get; set; }
        List<MovableObject> IgnoreList { get; }
        void AddToIgnoreList(MovableObject obj);
        Func<Vector3, Vector3, Vector3> CollisionFunc { get; set; }
    }

    public static class Utils
    {
        public static Vector3 DefaultCollisionDetectionFunction(this ICollidableCamera collidableCamera, Vector3 cameraTargetPosition, Vector3 cameraPosition)
        {
            Vector3 finalCameraPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);

            var origin = cameraTargetPosition;
            var direction = (cameraPosition - cameraTargetPosition);
            float maxDistance = direction.Length;
            direction = direction.NormalisedCopy;

            var raySceneQuery = collidableCamera.CameraCS.SceneManager.CreateRayQuery(new Ray(origin, direction));
            Debug.Assert(raySceneQuery != null);
            var reslut = raySceneQuery.Execute();
            var itr = reslut.GetEnumerator();

            bool intersect = false;
            float minDistance = float.MaxValue;
            while (itr.MoveNext()) {
                var current = itr.Current;
                Debug.Assert(current != null);
                if (current.distance < maxDistance
                    && current.distance < minDistance
                    && current.movable != null
                    && current.movable.ParentSceneNode != null
                    && current.movable.ParentSceneNode != collidableCamera.CameraCS.CameraSceneNode
                    && current.movable.ParentSceneNode != collidableCamera.CameraCS.TargetNode
                    && !collidableCamera.IgnoreList.Contains(current.movable)) {
                    minDistance = current.distance;
                    intersect = true;
                    if (current.worldFragment != null) {
                        finalCameraPosition = current.worldFragment.singleIntersection;
                    } else {
                        // if margin = 0 the floor in the demo always collides with the camera once it has been touched
                        finalCameraPosition = origin + (direction * current.distance);
                    }
                }
            }

            collidableCamera.CameraCS.SceneManager.DestroyQuery(raySceneQuery);
            return finalCameraPosition;
        }

    }

    /*
    public class CollidableCamera
            //:CameraMode
        {
            protected CameraControlSystem mCameraCS2;
            protected bool mCollisionsEnabled;
            protected float _margin;
            protected List<Mogre.MovableObject> _ignoreList;

            public ICollidableCamera(CameraControlSystem cam, float margin = 0.1f)
            {
                mCameraCS2 = cam;
                mCollisionsEnabled = false;
                _margin = margin;
            }

            //todo change to C# style
            public virtual void setCollisionsEnabled(bool value)
            {
                mCollisionsEnabled = value;
            }
            public bool getCollisionsEnabled()
            {
                return mCollisionsEnabled;
            }

            public void setMargin(float margin)
            {
                _margin = margin;
            }
            public float getMargin()
            {
                return _margin;
            }

            //todo merge collision delegates later

            public void AddToIgnoreList(MovableObject obj)
            {
                _ignoreList.Add(obj);
            }

           public  Func<Vector3,Vector3,Vector3> newCollisionDelegate()
            {
                return DefaultCollisionDetectionFunction;
            }

            public Vector3 DefaultCollisionDetectionFunction(Vector3 cameraTargetPosition, Vector3 cameraPosition)
            {
                Vector3 finalCameraPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);

                var origin = cameraTargetPosition;
                var direction = (cameraPosition - cameraTargetPosition);
                float maxDistance = direction.Length;
                direction = direction.NormalisedCopy;

                var raySceneQuery = mCameraCS2.getSceneManager().CreateRayQuery(new Ray(origin, direction));
                var reslut = raySceneQuery.Execute();
                var itr = reslut.GetEnumerator();

                bool intersect = false;
                float minDistance = float.MaxValue;
                while (itr.MoveNext()) {
                    var current = itr.Current;
                    if (current.distance < maxDistance
                        && current.distance < minDistance
                        && current.movable.ParentSceneNode != mCameraCS2.CameraSceneNode()
                        && current.movable.ParentSceneNode != mCameraCS2.getTargetSceneNode()
                        && !_ignoreList.Contains(current.movable)) {
                        minDistance = current.distance;
                        intersect = true;
                        if (current.worldFragment != null) {
                            finalCameraPosition = current.worldFragment.singleIntersection;
                        } else {
                            // if margin = 0 the floor in the demo always collides with the camera once it has been touched
                            finalCameraPosition = origin + (direction * current.distance);
                        }
                    }
                }

                mCameraCS2.getSceneManager().DestroyQuery(raySceneQuery);
                return finalCameraPosition;
            }

        }

        //*/
}
