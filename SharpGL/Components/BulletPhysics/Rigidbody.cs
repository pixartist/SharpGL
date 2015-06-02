using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

using SharpGL.Drawing;
using BulletSharp;
namespace SharpGL.Components.BulletPhysics
{
	public class Rigidbody : Component
	{
        public const float DefaultMass = 1.0f;
        public const float DefaultAngularDamping = 0.1f;
        public const float DefaultLinearDamping = 0.1f;
        public const float DefaultFriction = 0.5f;
		public BulletSharp.RigidBody Body { get; private set; }
		public Vector3 Center { get; private set; }
		public Vector3 Shift { get { return Vector3.Transform(Center, Transform.Rotation); } }
        public Vector3 Scale { get; private set; }
		protected override void OnInit()
		{
		}
		private void ClearBody()
		{
			if (Body != null)
				GameObject.App.PhysicsWorld.RemoveRigidBody(Body);
			Body = null;
		}
		public void SetCollisionBox(Vector3 center, Vector3 scale)
		{
            SetCollisionShape(new BoxShape(0.5f), center, scale);
		}
		public void SetCollisionCapsule(float length, float radius, Vector3 center)
		{
            SetCollisionShape(new CapsuleShape(radius, length), center, Vector3.One);
		}
		public void SetCollisionConvexMesh(Mesh mesh, Vector3 scale)
		{
			if (mesh == null)
			{
				Log.Error("Could not apply mesh to rigidbody! GameObject: " + GameObject.Name);
			}
			else
			{
                ConvexHullShape hull = new ConvexHullShape(mesh.GetPoints());
                SetCollisionShape(hull, Vector3.Zero, scale);
			}
		}
        public void SetCollisionConvexMesh(Mesh mesh, Vector3 centerOfMass, Vector3 scale)
        {
            if (mesh == null)
            {
                Log.Error("Could not apply mesh to rigidbody! GameObject: " + GameObject.Name);
            }
            else
            {
                
                ConvexHullShape hull = new ConvexHullShape(mesh.GetPoints());
                CompoundShape cs = new CompoundShape();
                cs.AddChildShape(Matrix4.CreateTranslation(-centerOfMass), hull);
                SetCollisionShape(cs, centerOfMass, scale);
                
            }
        }
		public void SetCollisionShape(CollisionShape shape, Vector3 center, Vector3 scale)
		{
			Center = center;
			ClearBody();
            
            //RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(DefaultMass, new DefaultMotionState(Transform.GetMatrixInverse()), shape, shape.CalculateLocalInertia(DefaultMass));
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(DefaultMass, new RbMotionState(Transform, center, Vector3.Zero), shape, shape.CalculateLocalInertia(DefaultMass));
            info.AngularDamping = DefaultAngularDamping;
            info.LinearDamping = DefaultLinearDamping;
            info.Friction = DefaultFriction;

            SetRigidBody(info, scale);
		}
        public void SetRigidBody(RigidBodyConstructionInfo info, Vector3 scale)
        {
            Body = new RigidBody(info);
            Body.UserObject = GameObject;
            Body.CollisionShape.LocalScaling = scale;
            GameObject.App.PhysicsWorld.AddRigidBody(Body);
        }
		protected override void OnUpdate(float dt)
		{
            //Transform.Rotation = Body.Orientation;
            //Transform.Position = Body.CenterOfMassPosition - Vector3.Transform(Center, Transform.Rotation);
            
            
		}
        public void MakeStatic()
        {
            if (Body != null)
                Body.SetMassProps(0, Vector3.Zero);
        }
        public void MakeDynamic(float mass = DefaultMass)
        {
            if (mass <= 0)
                throw (new ArgumentException("Mass has to be > 0"));
            if (Body != null)
            {
                Body.SetMassProps(mass, Body.CollisionShape.CalculateLocalInertia(mass));
            }
        }
        public void ApplyLinearImpulse(Vector3 force)
        {
            if(Body != null)
                Body.ApplyCentralImpulse(force);
        }
        public void ApplyLinearImpulse(Vector3 force, Vector3 localPosition)
        {
            if (Body != null)
                Body.ApplyImpulse(force, localPosition);
        }
        public void ApplyAngularImpulse(Vector3 torque)
        {
            if (Body != null)
            {
                Body.ApplyTorqueImpulse(torque);
                //Body.AngularVelocity = torque;
            }
        }
        public void SetDamping(float linearDamping = DefaultLinearDamping, float angularDamping = DefaultAngularDamping)
        {
            if(Body != null)
            {
                Body.SetDamping(linearDamping, angularDamping);
            }
        }
        public void SetMass(float mass)
        {
            if (Body != null)
                Body.SetMassProps(mass, Vector3.One);
        }
		public override void OnDestroy()
		{
			ClearBody();
		}
	}
}
