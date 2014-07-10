using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SharpGL.Drawing;
using SharpGL.Components;
namespace SharpGL.Components
{
	public class Camera : Component
	{
		private Matrix4 projectionMatrix;
		public Vector3 PositionTarget { get; set; }
		public Quaternion RotationTarget { get; set; }
		public float TransAccel { get; set; }
		public float RotAccel { get; set; }
		public bool LerpRotation { get; set; }
		public bool LerpTranslation { get; set; }
		public bool PitchLock { get; set; }
		private float zNear;
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
		private float zFar;
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
		private float fov;
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
				float w = 2 * Mathf.Tan(fov / 2) * ZNear;
				return new Vector2(w, w / AspectRatio);
			}
		}
		public float AspectRatio { get; private set; }
		public Camera(GameObject parent) : base	(parent)
		{

		}
		internal override void Init()
		{
			RotationTarget = Quaternion.Identity;
			LerpRotation = false;
			LerpTranslation = true;
			TransAccel = 1;
			RotAccel = 1;
			PitchLock = true;
			fov = 90;
			zNear = 1.0f;
			zFar = 120f;
			SetupProjection();
		}
		public Matrix4 GetModelViewProjectionMatrix(Transform model)
		{
			Matrix4 m = model.GetMatrix();
			Matrix4 v = Transform.GetViewMatrix();
			Matrix4 p = projectionMatrix;

			//p.Transpose();
			return m * v * p;
		}
		public void Update(float tDelta)
		{
			if(LerpTranslation)
				Transform.Position += (PositionTarget - Transform.Position) * TransAccel * tDelta;
			if (LerpRotation)
			{
				Transform.Rotation = Quaternion.Slerp(Transform.Rotation, RotationTarget, RotAccel * tDelta);
				
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
				float sign =  Math.Sign(Transform.Forward.Y);
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
			Vector3 tmp = Vector3.Transform(world, (GameObject.Transform.GetMatrix() * projectionMatrix));
			tmp.X = (tmp.X + 1) * 0.5f * GameObject.App.Window.Width;
			tmp.Y = (1 - tmp.Y) * 0.5f * GameObject.App.Window.Height;
			return new Vector2(tmp.X, tmp.Y);
		}
		
		public Vector3 ScreenToDirection(Vector2 screen)
		{
			Vector2 ss = new Vector2(GameObject.App.Window.Width, -GameObject.App.Window.Height);
			screen.X /= ss.X;
			screen.Y /= ss.Y;
			screen -= new Vector2(0.5f, -0.5f);
			screen.Y /= AspectRatio;
			screen *= 2 * Mathf.Tan(fov / 2) * ZNear;
			//screen.Y /= AspectRatio;
			//screen *= Mathf.Deg2Rad(Fov);
			//screen.Y /= AspectRatio;
			//Quaternion fw = Transform.Rotation;
			//fw *= Quaternion.FromAxisAngle(Transform.Right, screen.Y);
			//fw *= Quaternion.FromAxisAngle(Transform.Up, screen.X);
			
			/*float x = screen.X / GameO;
			float y = -1 * (1.0f - screen.Y / (GameObject.App.Window.Height * 0.5f));
			float dx = Mathf.Tan(Fov * 0.5f) * x / AspectRatio;
			float dy = Mathf.Tan(Fov * 0.5f) * y;
			Matrix4 inv = GameObject.Transform.GetMatrix().Inverted();
			Vector3 p1 = new Vector3(dx * ZNear, dy * ZNear, ZNear);
			Vector3 p2 = new Vector3(dx * ZFar, dy * ZFar, ZFar);
			p1 = Vector3.Transform(p1, inv);
			p2 = Vector3.Transform(p2, inv);
			return (p2 - p1).Normalized();*/
			//Console.WriteLine(Transform.Position);
			//Log.Write(fw.ToString());
			//var res = Vector3.Transform(-Vector3.UnitZ, fw.Conjugated());
			return Transform.RotateBy(new Vector3(screen.X, screen.Y, -1).Normalized());
		}
		public Vector3 ScreenToWorld(Vector2 screen)
		{
			return GameObject.Transform.Position + ScreenToDirection(screen);
		}
	}
}
