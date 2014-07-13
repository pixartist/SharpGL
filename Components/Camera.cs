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
		private bool beganDraw;
		private float zNear;
		private float zFar;
		private float fov;
		protected Matrix4 projectionMatrix;
		public Material CameraMaterial { get; private set; }
		private Mesh screenMesh;
		public int VAO { get; private set; }

		public Shader CameraShader
		{
			get
			{
				if (CameraMaterial == null)
					return null;
				else
					return CameraMaterial.Shader;
			}
		}
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
			zNear = 1.0f;
			zFar = 120f;
			screenMesh = new Mesh();
			screenMesh.SetVertices(new float[] {
				-1,-1,
				1,-1,
				1,1,
				-1,1
			});
			screenMesh.SetIndices(new uint[] {
				2,3,0,
				0,1,2
			});
			screenMesh.SetDrawHints(new VertexObjectDrawHint("pos", 2, 2, 0, false));
			screenMesh.UpdateBuffers();
			VAO = GL.GenVertexArray();
			SetupProjection();
		}
		public Matrix4 GetModelViewProjectionMatrix(Transform model)
		{
			Matrix4 s = Matrix4.CreateScale(model.Scale);
			Matrix4 t = Matrix4.CreateTranslation(model.Position - Transform.Position);
			Matrix4 r = Matrix4.CreateFromQuaternion(Transform.Rotation * model.Rotation.Inverted());
			if (float.IsNaN(r.M11))
				r = Matrix4.Identity;
			Matrix4 p = projectionMatrix;

			//p.Transpose();
			return s * t * r * p;
		}
		public void Update(float tDelta)
		{
			if(LerpTranslation)
				Transform.LocalPosition += (PositionTarget - Transform.LocalPosition) * TransAccel * tDelta;
			if (LerpRotation)
			{
				Transform.LocalRotation = Quaternion.Slerp(Transform.LocalRotation, RotationTarget, RotAccel * tDelta);
				
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
		public void SetCameraShader(Shader shader, int multisampling)
		{
			if (CameraMaterial != null)
			{
				CameraMaterial.Dispose();
				CameraMaterial = null;
			}
			if(shader != null)
			{

				Surface bufferSurface = new Surface();
				bufferSurface.Create(GameObject.App.Window.Width, GameObject.App.Window.Height, new Surface.SurfaceFormat { DepthBuffer = true, WrapMode = TextureWrapMode.Clamp, Multisample = multisampling});
				CameraMaterial = new Material(shader, new Material.Texture("tex", bufferSurface));
				
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
		public void BeginDraw()
		{
			if(CameraMaterial != null)
			{
				CameraMaterial.Textures[0].Surface.Clear();
				CameraMaterial.Textures[0].Surface.BindFramebuffer();
				beganDraw = true;
			}
		}
		public void EndDraw()
		{
			if (beganDraw)
			{
				beganDraw = false;
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				if (CameraMaterial != null)
				{
					GL.BindBuffer(BufferTarget.ArrayBuffer, screenMesh.VBO);
					GL.BindVertexArray(VAO);
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, screenMesh.VEO);
					CameraMaterial.Use();
					screenMesh.ApplyDrawHints(CameraMaterial.Shader);
					GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					GL.BindVertexArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					GL.UseProgram(0);
				}
			}
		}
		public Vector2 WorldToScreen(Vector3 world)
		{
			/*Vector3 tmp = Vector3.Transform(world, (GameObject.Transform.GetMatrix() * projectionMatrix));
			tmp.X = (tmp.X + 1) * 0.5f * GameObject.App.Window.Width;
			tmp.Y = (1 - tmp.Y) * 0.5f * GameObject.App.Window.Height;
			return new Vector2(tmp.X, tmp.Y);*/
			throw (new NotImplementedException());
		}
		
		public Vector3 ScreenToDirection(Vector2 screen)
		{
			Vector2 ss = new Vector2(GameObject.App.Window.Width, -GameObject.App.Window.Height);
			screen.X /= ss.X;
			screen.Y /= ss.Y;
			screen -= new Vector2(0.5f, -0.5f);
			screen.Y /= AspectRatio;
			screen *= NearplaneSize.X;
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
			return GameObject.Transform.LocalPosition + ScreenToDirection(screen);
		}
	}
}
