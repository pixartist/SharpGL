using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using BulletSharp;
namespace SharpGL.Components.BulletPhysics
{
    public class RbMotionState : MotionState
    {
        private Transform objectTransform;
        public Vector3 Center { get; set; }
        public Vector3 CenterOfGravity { get; set; }
        public override Matrix4 WorldTransform
        {
            get
            {
                return objectTransform.GetMatrixInverse() * Matrix4.CreateTranslation(Center);
            }
            set
            {
                objectTransform.Rotation = value.ExtractRotation();
                objectTransform.Position = value.ExtractTranslation() - Vector3.Transform(Center, objectTransform.Rotation);
            }
        }
        public RbMotionState(Transform tranform, Vector3 center, Vector3 centerOfGravity)
        {
            Center = center;
            CenterOfGravity = centerOfGravity;
            objectTransform = tranform;
            WorldTransform = tranform.GetMatrixInverse();
        }
    }
}
