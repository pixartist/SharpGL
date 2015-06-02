using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SharpGL.Components;
using OpenTK;
namespace SharpGL
{
    public class Ray
    {
        private static App _app;
        /// <summary>
        /// Initialized the raycaster. Should only be called once. (Called automatically by the App class)
        /// </summary>
        /// <param name="app"></param>
        public static void Init(App app)
        {
            _app = app;
        }
        /// <summary>
        /// Casts a ray and returns information about the first object hit
        /// </summary>
        /// <param name="origin">The origin of the ray</param>
        /// <param name="target">The target of the ray</param>
        /// <returns>A Ray object if the ray hits and Object, null otherwise</returns>
        public static Ray Cast(Vector3 origin, Vector3 target)
        {
            var cb  = new ClosestRayResultCallback(origin, target);
            _app.PhysicsWorld.RayTest(origin, target, cb);
            if(cb.HasHit)
                return new Ray(cb.CollisionObject.UserObject as GameObject, origin, target, cb.HitPointWorld, cb.HitNormalWorld);
            return null;
        }
        /// <summary>
        /// References the GameObject owning the Physics Object hit by the ray
        /// </summary>
        public GameObject HitObject { get; private set; }
        /// <summary>
        /// The world origin of the ray
        /// </summary>
        public Vector3 Origin { get; private set; }
        /// <summary>
        /// The world target of the ray
        /// </summary>
        public Vector3 Target { get; private set; }
        /// <summary>
        /// The world location of the ray hit
        /// </summary>
        public Vector3 HitPoint { get; private set; }
        /// <summary>
        /// The world normal of the ray hit
        /// </summary>
        public Vector3 HitNormal { get; private set; }
        private Ray(GameObject hit, Vector3 origin, Vector3 target, Vector3 hitPoint, Vector3 hitNormal)
        {
            this.HitObject = hit;
            this.Origin = origin;
            this.Target = target;
            this.HitPoint = hitPoint;
            this.HitNormal = HitNormal;
        }
    }
}
