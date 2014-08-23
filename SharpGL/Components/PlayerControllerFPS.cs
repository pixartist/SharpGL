using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.JitterHelper;
namespace SharpGL.Components
{
	public class PlayerControllerFPS : Rigidbody
	{
		public Vector3 ForwardHorizontal
		{
			get
			{
				Vector3 fw = Transform.LocalForward;
				fw.Y = 0;
				return fw.Normalized();
			}
		}
		public float Acceleration { get; set; }
		public float Drag { get; set; }
		protected override void OnInit()
		{
			base.OnInit();
			SetCollisionCapsule(1.8f, 0.2f, new Vector3(0.1f, 0.15f, 0.1f));
			Body.Material.Restitution = 0;
			Body.Material.KineticFriction = 0.0f;
			Body.Material.StaticFriction = 0.0f;
			Body.Damping = Jitter.Dynamics.RigidBody.DampingType.None;
			Body.Mass = 80;
			Acceleration = 8f;
			Drag = 0.02f;
		}
		public void MoveForwardHorizontal(float strength = 1.0f)
		{
			Translate(ForwardHorizontal, strength);
		}
		public void MoveBackHorizontal(float strength = 1.0f)
		{
			Translate(ForwardHorizontal, -strength);
		}
		public void MoveForward(float strength = 1.0f)
		{
			Translate(Transform.LocalForward, strength);
		}
		public void MoveRight(float strength = 1.0f)
		{
			Translate(Transform.LocalRight, strength);
		}
		public void MoveUp(float strength = 1.0f)
		{
			Translate(Transform.LocalUp, strength);
		}
		public void MoveBack(float strength = 1.0f)
		{
			Translate(Transform.LocalForward, -strength);
		}
		public void MoveLeft(float strength = 1.0f)
		{
			Translate(Transform.LocalRight, -strength);
		}
		public void MoveDown(float strength = 1.0f)
		{
			Translate(Transform.LocalUp, -strength);
		}
		public void Translate(Vector3 direction, float strength = 1.0f)
		{
			direction.Normalize();
			Body.ApplyImpulse((direction * Acceleration * strength).ToJVector());
		}
		protected override void OnUpdate(float dt)
		{
			Jitter.Dynamics.RigidBody body;
			float fraction;
			Jitter.LinearMath.JVector normal;
			App.PhysicsWorld.CollisionSystem.Raycast(Body.Position, Jitter.LinearMath.JVector.Down, null, out body, out normal, out fraction);
			//Console.WriteLine(fraction + " " + Body.IsActive);
			Body.LinearVelocity *= 1 - Drag;
			var a = Vector3.CalculateAngle(Vector3.UnitY, Transform.LocalUp);
			var fw = Transform.LocalForward;
			fw.Y = 0;
			var rot = fw.LookDirection(Vector3.UnitY);
			//Transform.Position += Body.LinearVelocity.ToVector3() * 0.01f;
			Body.AngularVelocity = new Jitter.LinearMath.JVector(0, 0, 0);
			Body.Orientation = Quaternion.Slerp(Body.Orientation.ToQuaternion(), rot, 0.1f).ToJMatrix();
			//Body.Position = (Transform.Position + Shift).ToJVector();
			Transform.Position = Body.Position.ToVector3() - Vector3.Transform(Center, rot);
			//base.OnUpdate();
		}
	}
}
