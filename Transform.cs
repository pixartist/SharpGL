using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.Components;
namespace SharpGL
{
	
	public static class Extensions
	{
		public static float PI = (float)Math.PI;
		public static Quaternion RotationBetweenVectors(Vector3 u, Vector3 v)
		{
			float m = (float)Math.Sqrt(2.0f + 2.0f * Vector3.Dot(u, v));
			Vector3 w = (1.0f / m) * Vector3.Cross(u, v);
			return new Quaternion(0.5f * m, w.X, w.Y, w.Z);
		}
		
		public static Quaternion LookDirection(this Vector3 direction, Vector3 up)
		{
			return Matrix4.LookAt(direction, Vector3.Zero, Vector3.UnitY).ToQuaternion().Inverted();
		}
		public static Quaternion LookAt(this Vector3 from, Vector3 target, Vector3 up)
		{
			return Matrix4.LookAt(from, target, Vector3.UnitY).ToQuaternion().Inverted();
		}
		public static Matrix4 ToMatrix4(this Quaternion q)
		{
			Matrix4 result = Matrix4.Identity;

			float X = q.X;
			float Y = q.Y;
			float Z = q.Z;
			float W = q.W;

			float xx = X * X;
			float xy = X * Y;
			float xz = X * Z;
			float xw = X * W;
			float yy = Y * Y;
			float yz = Y * Z;
			float yw = Y * W;
			float zz = Z * Z;
			float zw = Z * W;

			result.M11 = 1 - 2 * (yy + zz);
			result.M21 = 2 * (xy - zw);
			result.M31 = 2 * (xz + yw);
			result.M12 = 2 * (xy + zw);
			result.M22 = 1 - 2 * (xx + zz);
			result.M32 = 2 * (yz - xw);
			result.M13 = 2 * (xz - yw);
			result.M23 = 2 * (yz + xw);
			result.M33 = 1 - 2 * (xx + yy);
			return result;
		}
		public static Quaternion ToQuaternion(this Matrix4 m)
		{
			Quaternion q;

			float trace = 1 + m.M11 + m.M22 + m.M33;
			float S = 0;
			float X = 0;
			float Y = 0;
			float Z = 0;
			float W = 0;

			if (trace > 0.0000001)
			{
				S = (float)Math.Sqrt(trace) * 2;
				X = (m.M23 - m.M32) / S;
				Y = (m.M31 - m.M13) / S;
				Z = (m.M12 - m.M21) / S;
				W = 0.25f * S;
			}
			else
			{
				if (m.M11 > m.M22 && m.M11 > m.M33)
				{
					// Column 0: 
					S = (float)Math.Sqrt(1.0 + m.M11 - m.M22 - m.M33) * 2;
					X = 0.25f * S;
					Y = (m.M12 + m.M21) / S;
					Z = (m.M31 + m.M13) / S;
					W = (m.M23 - m.M32) / S;
				}
				else if (m.M22 > m.M33)
				{
					// Column 1: 
					S = (float)Math.Sqrt(1.0 + m.M22 - m.M11 - m.M33) * 2;
					X = (m.M12 + m.M21) / S;
					Y = 0.25f * S;
					Z = (m.M23 + m.M32) / S;
					W = (m.M31 - m.M13) / S;
				}
				else
				{
					// Column 2:
					S = (float)Math.Sqrt(1.0 + m.M33 - m.M11 - m.M22) * 2;
					X = (m.M31 + m.M13) / S;
					Y = (m.M23 + m.M32) / S;
					Z = 0.25f * S;
					W = (m.M12 - m.M21) / S;
				}
			}
			q = new Quaternion(X, Y, Z, W);
			return q;
		}
		public static Vector3 ToEuler(this Quaternion q)
		{
			Vector3 euler = new Vector3();
			double sqw = q.W * q.W;
			double sqx = q.X * q.X;
			double sqy = q.Y * q.Y;
			double sqz = q.Z * q.Z;
			double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			double test = q.X * q.Y + q.Z * q.W;
			if (test > 0.499 * unit)
			{ // singularity at north pole
				euler.X = (float)(2 * Math.Atan2(q.X, q.W));
				euler.Y = (float)(PI / 2);
				euler.Z = 0;
				return euler;
			}
			if (test < -0.499 * unit)
			{ // singularity at south pole
				euler.X = -2 * (float)(2 * Math.Atan2(q.X, q.W));
				euler.Y = (float)(-PI / 2);
				euler.Z = 0;
				return euler;
			}
			euler.X = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw); ;
			euler.Y = (float)Math.Asin(2 * test / unit);
			euler.Z = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw);
			return euler;
		}
		public static Quaternion ToQuaternion(this Vector3 euler)
		{
			// Assuming the angles are in radians.
			double c1 = Math.Cos(euler.X / 2);
			double s1 = Math.Sin(euler.X / 2);
			double c2 = Math.Cos(euler.Y / 2);
			double s2 = Math.Sin(euler.Y / 2);
			double c3 = Math.Cos(euler.Z / 2);
			double s3 = Math.Sin(euler.Z / 2);
			double c1c2 = c1 * c2;
			double s1s2 = s1 * s2;
			return new Quaternion((float)(c1c2 * s3 + s1s2 * c3), (float)(s1 * c2 * c3 + c1 * s2 * s3), (float)(c1 * s2 * c3 - s1 * c2 * s3), (float)(c1c2 * c3 - s1s2 * s3));
		}
		public static Quaternion Conjugated(this Quaternion me)
		{
			Quaternion c = me;
			c.Conjugate();
			return c;
		}
	}

	public class Transform : Component
	{
		public Transform Parent
		{
			get
			{
				if (GameObject != null)
					if (GameObject.Parent != null)
						if (GameObject.Parent.Transform != null)
							return GameObject.Parent.Transform;
				return null;
			}
		}
		public static float PI = (float)Math.PI;
		public virtual Vector3 LocalPosition { get; set; }
		public virtual Vector3 Position
		{
			get
			{
				Transform p;
				if (TryGetParent(out p))
				{
					return Vector3.Add(p.Position, LocalPosition);
				}
				return LocalPosition;
			}
		}
		public virtual Vector3 LocalScale { get; set; }
		public virtual Vector3 Scale
		{
			get
			{
				Transform p;
				if(TryGetParent(out p))
				{
					return Vector3.Multiply(p.Scale, LocalScale);
				}
				return LocalScale;
			}
		}
		private Quaternion _rotation;
		public virtual Quaternion Rotation
		{
			get
			{
				Transform p;
				if (TryGetParent(out p))
				{
					return Quaternion.Multiply(p.Rotation, LocalRotation);
				}
				return LocalRotation;
			}
		}
		public virtual Quaternion LocalRotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = value;
				_rotation.Normalize();
			}
		}
		public virtual Vector3 Right
		{
			get
			{

				return Vector3.Transform(Vector3.UnitX, LocalRotation);
			}
		}
		public virtual Vector3 Forward
		{
			get
			{
				return Vector3.Transform(-Vector3.UnitZ, LocalRotation);
			}
		}
		public virtual Vector3 Up
		{
			get
			{
				return Vector3.Transform(Vector3.UnitY, LocalRotation);
			}
		}
		public virtual Vector3 EulerAngles
		{
			get
			{
				return LocalRotation.ToEuler();
			}
		}
		internal override void Init()
		{
			LocalPosition = Vector3.Zero;
			LocalRotation = Quaternion.Identity;
			LocalScale = Vector3.One;
		}
		public bool TryGetParent(out Transform parent)
		{
			parent = Parent;
			return parent != null;
		}
		public virtual Matrix4 GetMatrix()
		{
			Matrix4 translation = Matrix4.CreateTranslation(Position);
			Matrix4 rotation = Matrix4.CreateFromQuaternion(Rotation);
			Matrix4 scale = Matrix4.CreateScale(LocalScale);
			return scale * rotation * translation;
		}
		public virtual void Translate(Vector3 amount)
		{
			LocalPosition += amount;
		}
		public virtual void TranslateLocal(Vector3 amount)
		{
			LocalPosition += Vector3.Transform(amount, LocalRotation);
		}
		public virtual void Rotate(Vector3 axis, float angle)
		{
			LocalRotation = Quaternion.FromAxisAngle(axis, angle) * LocalRotation;
		}
	}
}

