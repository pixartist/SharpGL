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
    /// <summary>
    /// The Camera class implements a virtual, configurable camera.
    /// </summary>
	public class Camera : Component
	{
		
		
		private float zNear;
		private float zFar;
		private float fov;
		
		protected Matrix4 projectionMatrix;

        /// <summary>
        /// If set to true, camera pitch will be locked at -90 & 90 degrees (down & up)
        /// </summary>
		public bool PitchLock { get; set; }
		
		/// <summary>
		/// Defines how far the nearplane of the camera is from the camera location
		/// </summary>
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
		/// <summary>
		/// Defines how far the farplane of the camera is from the camera location
		/// </summary>
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
		/// <summary>
		/// Defines the horizontal field of view of the camera (in angles)
		/// </summary>
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
        /// <summary>
        /// Returns the size in world coordinates of the nearplane
        /// </summary>
		public Vector2 NearplaneSize
		{
			get
			{
				float h = 2 * Mathf.Tan(Mathf.Deg2Rad(Fov) / 2) * ZNear;
				return new Vector2(h * AspectRatio, h);
			}
		}
        /// <summary>
        /// Defines the aspect ratio of the camera projection
        /// </summary>
		public float AspectRatio { get; private set; }
		protected override void OnInit()
		{
			PitchLock = true;
			fov = 90;
			zNear = 0.1f;
			zFar = 120f;
			SetupProjection();
			
		}
        /// <summary>
        /// Creates a model-view-projection-matrix for the given transform
        /// </summary>
        /// <param name="model">The transform that will be used to construct the matrix</param>
        /// <returns>A Matrix representing the transformation of the given transform for the current camera view</returns>
		public Matrix4 GetModelViewProjectionMatrix(Transform model)
		{
			Matrix4 m = model.GetMatrix();
			Matrix4 v = Transform.GetMatrix().Inverted();
			Matrix4 p = projectionMatrix;
			return m * v * p;
		}
		
		
		
		/// <summary>
		/// Rotates the camera along the given axis
		/// </summary>
        /// <param name="axis">The axis in global space</param>
		/// <param name="angle">The angle in radians</param>
		public void Rotate(Vector3 axis, float angle)
		{
			Transform.Rotate(axis, angle);
			if (PitchLock)
			{
				float a = Vector3.CalculateAngle(Vector3.UnitY, Transform.LocalUp);
				float sign =  -Math.Sign(Transform.LocalForward.Y);
				float delta = a - ((float)Math.PI / 2 - 0.1f);
				if (delta > 0)
					Transform.Rotate(Transform.LocalRight, delta * sign);
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
        /// <summary>
        /// Transforms world coordinates to screen coordinates for this camera view
        /// </summary>
        /// <param name="world">A point in the world</param>
        /// <returns>The location of the given point on the screen</returns>
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
		/// <summary>
		/// Transforms a screen location into world coordinates
		/// </summary>
		/// <param name="screen">A point on the screen</param>
		/// <returns>A direction vector corresponing to the direction of the given point on the screen</returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screen"></param>
        /// <returns>A point corresponding to the given point on the screen, in world coordinates, with a distance of 1 to the camera location</returns>
		public Vector3 ScreenToWorld(Vector2 screen)
		{
			return GameObject.Transform.LocalPosition + ScreenToDirection(screen);
		}
	}
}
