﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.Components;
namespace SharpGL.Components
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
			return Matrix4.LookAt(direction, Vector3.Zero, Vector3.UnitY).ToQuaternion();
		}
		public static Quaternion LookAt(this Vector3 from, Vector3 target, Vector3 up)
		{
			return Matrix4.LookAt(from, target, Vector3.UnitY).ToQuaternion();
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
    /// <summary>
    /// A Transform holds and manages the transformation data for every GameObject
    /// </summary>
	public class Transform : Component
	{
        /// <summary>
        /// Returns the parent of the owning GameObject
        /// </summary>
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
        /// <summary>
        /// PI as floating point
        /// </summary>
		public static float PI = (float)Math.PI;
        /// <summary>
        /// The local position of the Transform
        /// </summary>
		public virtual Vector3 LocalPosition { get; set; }
        /// <summary>
        /// The global position of the Transform
        /// </summary>
		public virtual Vector3 Position
		{
			get
			{
				Transform p;
				if (TryGetParent(out p))
				{
					return Vector3.Add(p.Position, Vector3.Transform(LocalPosition, p.Rotation));
				}
				return LocalPosition;
			}
			set
			{
				Transform p;
				if (TryGetParent(out p))
				{
					LocalPosition = value - p.Position;
				}
				else
				{
					LocalPosition = value;
				}
			}
		}
        /// <summary>
        /// The local scale of the Transform
        /// </summary>
		public virtual Vector3 LocalScale { get; set; }
        /// <summary>
        /// The global scale of the Transform
        /// </summary>
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
        /// <summary>
        /// The global rotation of the Transform
        /// </summary>
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
			set
			{
				Transform p;
				if (TryGetParent(out p))
				{
					LocalRotation = Quaternion.Multiply(value, p.Rotation.Inverted());
				}
				else
				{
					LocalRotation = value;
				}
			}
		}
        /// <summary>
        /// The local rotation of the Transform
        /// </summary>
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
        /// <summary>
        /// The Vector3 pointing to the local right
        /// </summary>
		public virtual Vector3 LocalRight
		{
			get
			{

				return Vector3.Transform(Vector3.UnitX, LocalRotation);
			}
		}
        /// <summary>
        /// The Vector3 pointing to the local forward
        /// </summary>
		public virtual Vector3 LocalForward
		{
			get
			{
				return Vector3.Transform(-Vector3.UnitZ, LocalRotation);
			}
		}
        /// <summary>
        /// The Vector3 pointing to the local up
        /// </summary>
		public virtual Vector3 LocalUp
		{
			get
			{
				return Vector3.Transform(Vector3.UnitY, LocalRotation);
			}
		}
        /// <summary>
        /// The euler angle representation of the local rotation
        /// </summary>
		public virtual Vector3 EulerAngles
		{
			get
			{
				return LocalRotation.ToEuler();
			}
		}
		protected override void OnInit()
		{
			LocalPosition = Vector3.Zero;
			LocalRotation = Quaternion.Identity;
			LocalScale = Vector3.One;
		}
        /// <summary>
        /// Tries to get the parent GameObject
        /// </summary>
        /// <param name="parent">The owning GameObject</param>
        /// <returns>True if the component has a parent, false otherwise</returns>
		public bool TryGetParent(out Transform parent)
		{
			parent = Parent;
			return parent != null;
		}
        /// <summary>
        /// Returns a Matrix representing the translation, rotation and scale of the Transform
        /// </summary>
        /// <returns>A Matrix representing the translation, rotation and scale of the Transform</returns>
		public virtual Matrix4 GetMatrix()
		{
			Matrix4 translation = Matrix4.CreateTranslation(Position);
			Matrix4 rotation = Matrix4.CreateFromQuaternion(Rotation);
			Matrix4 scale = Matrix4.CreateScale(Scale);
			return scale * rotation * translation;
		}
        /// <summary>
        /// Returns a Matrix representing the translation, rotation and scale of the Transform in a form fitting for GLSL
        /// </summary>
        /// <returns>A Matrix representing the translation, rotation and scale of the Transform in a form fitting for GLSL</returns>
        public virtual Matrix4 GetMatrixInverse()
        {
            Matrix4 translation = Matrix4.CreateTranslation(Position);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(Rotation);
            Matrix4 scale = Matrix4.CreateScale(Scale);
            return translation * rotation;
        }
        /// <summary>
        /// Translates the Transform by the given Value
        /// </summary>
        /// <param name="amount">The amount to translate by</param>
		public virtual void Translate(Vector3 amount)
		{
			Position += amount;
		}
        /// <summary>
        /// Translates the Transform by the given Value in local coordinates
        /// </summary>
        /// <param name="amount">The amount to translate by</param>
		public virtual void TranslateLocal(Vector3 amount)
		{
			LocalPosition += Vector3.Transform(amount, LocalRotation);
		}
        /// <summary>
        /// Rotates the Transform by the given angle around the given axis
        /// </summary>
        /// <param name="axis">The axis to rotate around</param>
        /// <param name="angle">The amount of rotation</param>
		public virtual void Rotate(Vector3 axis, float angle)
		{
			LocalRotation = Quaternion.FromAxisAngle(axis, angle) * LocalRotation;
		}
		
	}
}

