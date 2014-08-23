using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.Drawing;
using BulletSharp;
using SharpGL.JitterHelper;
namespace SharpGL.Components
{
	public class BulletRigidbody : Component
	{
	/*	public RigidBody Body { get; private set; }
		public Vector3 Center { get; private set; }
		public Vector3 Shift { get { return Vector3.Transform(Center, Transform.Rotation); } }
		public bool Static
		{
			get
			{
				if (Body == null)
					return true;
				return Body.IsStatic;
			}
			set
			{
				if (Body != null)
					Body.IsStatic = value;
			}
		}
		protected override void OnInit()
		{
		}
		private void ClearBody()
		{
			if (Body != null)
				GameObject.App.PhysicsWorld.RemoveBody(Body);
			Body = null;
		}
		public void SetCollisionBox(Vector3 scale, Vector3 center)
		{
			SetBody(new Jitter.Collision.Shapes.BoxShape(scale.ToJVector()), center);
		}
		public void SetCollisionCapsule(float length, float radius, Vector3 center)
		{
			SetBody(new Jitter.Collision.Shapes.CapsuleShape(length, radius), center);
		}
		public void SetCollisionConvexMesh(Mesh mesh, Vector3 scale, Vector3 center)
		{
			if (mesh == null)
			{
				Log.Error("Could not apply mesh to rigidbody! GameObject: " + GameObject.Name);
			}
			else
			{
				List<float> vertices = mesh.GetVertices();
				List<JVector> v = new List<JVector>();
				for (int i = 0; i + 2 < vertices.Count; i += 3)
				{
					v.Add(new JVector(vertices[i] * scale.X, vertices[i + 1] * scale.Y, vertices[i + 2] * scale.Z));
				}
				var shape = new Jitter.Collision.Shapes.ConvexHullShape(v);
				if (float.IsNaN(shape.Shift.X))
					throw (new InvalidOperationException("Invalid mesh used as collision body"));
				SetBody(shape, center);
			}
		}
		private void SetBody(Jitter.Collision.Shapes.Shape shape, Vector3 center)
		{
			Center = center;
			ClearBody();
			Body = new Jitter.Dynamics.RigidBody(shape);

			Body.Orientation = Transform.Rotation.ToJMatrix();
			Body.Position = (Transform.Position + Shift).ToJVector();
			GameObject.App.PhysicsWorld.AddBody(Body);
		}
		protected override void OnUpdate(float dt)
		{
			if (!Static)
			{
				Transform.Position = Body.Position.ToVector3() - Shift;
				Transform.Rotation = Body.Orientation.ToQuaternion();

			}
		}
		public override void OnDestroy()
		{
			ClearBody();
		}*/
	}
}
