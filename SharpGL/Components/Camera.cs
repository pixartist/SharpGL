using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.Drawing;
using SharpGL.Components;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Components
{
	public class Camera : Component
	{
		
		
		private float zNear;
		private float zFar;
		private float fov;
		
		protected Matrix4 projectionMatrix;
		public Vector3 PositionTarget { get; set; }
		public Quaternion RotationTarget { get; set; }
		public float TransAccel { get; set; }
		public float RotAccel { get; set; }
		public bool LerpRotation { get; set; }
		public bool LerpTranslation { get; set; }
		public bool PitchLock { get; set; }
		
		
		public float ZNear
		{
			get
			{
				return zNear;
			}
			set
			{
				zNear = value;
				SetupProjection();
			}
		}
		
		public float ZFar
		{
			get
			{
				return zFar;
			}
			set
			{
				zFar = value;
				SetupProjection();
			}
		}
		
		public float Fov
		{
			get
			{
				return fov;
			}
			set
			{
				fov = value;
				SetupProjection();
			}
		}
		public Vector2 NearplaneSize
		{
			get
			{
				float h = 2 * Mathf.Tan(Mathf.Deg2Rad(Fov) / 2) * ZNear;
				return new Vector2(h * AspectRatio, h);
			}
		}
		public float AspectRatio { get; private set; }
		internal override void Init()
		{
			RotationTarget = Quaternion.Identity;
			LerpRotation = false;
			LerpTranslation = true;
			TransAccel = 1;
			RotAccel = 1;
			PitchLock = true;
			fov = 90;
			zNear = 0.1f;
			zFar = 120f;
			SetupProjection();
			
		}
		public Matrix4 GetModelViewProjectionMatrix(Transform model)
		{
			Matrix4 m = model.GetMatrix();
			Matrix4 v = Transform.GetMatrix().Inverted();
			Matrix4 p = projectionMatrix;
			return m * v * p;
		}
		public void Update(float tDelta)
		{
			if(LerpTranslation)
				Transform.LocalPosition += (PositionTarget - Transform.LocalPosition) * Math.Min(1, tDelta * TransAccel);
			if (LerpRotation)
			{
				Transform.LocalRotation = Quaternion.Slerp(Transform.LocalRotation, RotationTarget, Math.Min(1, RotAccel * tDelta));
				
			}
		}
		public void MoveForward(float amount)
		{
			if (LerpTranslation)
			{
				TranslateTargetPosition(Transform.Forward * amount);
			}
			else
			{
				Transform.Translate(Transform.Forward * amount);
			}
		}
		public void MoveRight(float amount)
		{
			if (LerpTranslation)
			{
				TranslateTargetPosition(Transform.Right * amount);
			}
			else
			{
				Transform.Translate(Transform.Right * amount);
			}
		}
		public void MoveUp(float amount)
		{
			if (LerpTranslation)
			{
				TranslateTargetPosition(Transform.Up* amount);
			}
			else
			{
				Transform.Translate(Transform.Up * amount);
			}
		}
		public void Translate(Vector3 amount)
		{
			if (LerpTranslation)
			{
				TranslateTargetPosition(amount);
			}
			else
			{
				Transform.Translate(amount);
			}
		}
		private void TranslateTargetPosition(Vector3 amount)
		{
			PositionTarget += amount;
		}
		
		public void Rotate(Vector3 axis, float angle)
		{
			
			Transform.Rotate(axis, angle);
			if (PitchLock)
			{
				float a = Vector3.CalculateAngle(Vector3.UnitY, Transform.Up);
				float sign =  -Math.Sign(Transform.Forward.Y);
				float delta = a - (float)Math.PI / 2;
				if (delta > 0)
					Transform.Rotate(Transform.Right, delta * sign);
			}
		}
		
		private void SetupProjection()
		{
			if(GameObject != null)
			{
				AspectRatio = GameObject.App.Window.Width / (float)GameObject.App.Window.Height;
				projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)((Math.PI * Fov) / 180), AspectRatio, ZNear, ZFar);
			}
		}
		public Vector2 WorldToScreen(Vector3 world)
		{
			Matrix4 v = Transform.GetMatrix().Inverted();
			Matrix4 p = projectionMatrix;
			Vector4 wsv = Vector4.Transform(new Vector4(world, 1), v * p);
			wsv /= wsv.W;
			wsv.X = wsv.X + 1;
			wsv.Y = wsv.Y + 1;
			//Console.WriteLine(wsv);
			if(wsv.Z >= 1)
			{
				wsv.X -= GameObject.App.Window.Width;
				wsv.Y -= GameObject.App.Window.Height;
			}
			return new Vector2(wsv.X * GameObject.App.Window.Width, wsv.Y * GameObject.App.Window.Height);
		}
		
		public Vector3 ScreenToDirection(Vector2 screen)
		{
			Vector2 ss = new Vector2(GameObject.App.Window.Width, -GameObject.App.Window.Height);
			screen.X /= ss.X;
			screen.Y /= ss.Y;
			
			screen -= new Vector2(0.5f, -0.5f);
			screen.Y /= AspectRatio;
			screen *= NearplaneSize.X;
			return Vector3.Transform(new Vector3(screen.X, screen.Y, -ZNear).Normalized(), Transform.Rotation);
		}
		public Vector3 ScreenToWorld(Vector2 screen)
		{
			return GameObject.Transform.LocalPosition + ScreenToDirection(screen);
		}
	}
}
