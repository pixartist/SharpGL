using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SharpGL;
using SharpGL.Drawing;
using SharpGL.Input;
using SharpGL.Components;
using SharpGL.Factories;
using System.Threading;
namespace ModernShaders
{
	class ShaderGame : App
	{
		private GameObject rotator;
		private Gui gui;
		private Font defaultFont;
		private Material postEffect;
		private Surface multisampler;
		private MeshRenderer planeRenderer;
		private Canvas canvas;
		public ShaderGame(int width, int height) : base(width, height)
		{
			
		}
		public override void OnLoad()
		{
			float camSpeed = 0.05f;
			KeyboardHandler.RegisterKeyDown(Key.W, () => { ActiveCamera.MoveForward(camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.S, () => { ActiveCamera.MoveForward(-camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.A, () => { ActiveCamera.MoveRight(-camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.D, () => { ActiveCamera.MoveRight(camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.ShiftLeft, () => { ActiveCamera.Translate(new Vector3(0, -camSpeed, 0)); });
			KeyboardHandler.RegisterKeyDown(Key.Space, () => { ActiveCamera.Translate(new Vector3(0, camSpeed, 0)); });
			KeyboardHandler.RegisterKeyDown(Key.Escape, () => { Window.Close(); });
			KeyboardHandler.RegisterKeyDown(Key.Q, () => { ActiveCamera.Transform.LocalRotation = ActiveCamera.Transform.LocalPosition.LookAt(Vector3.Zero, Vector3.UnitY); });
			KeyboardHandler.RegisterKeyDown(Key.E, () =>
			{
				Vector2 sp = new Vector2(MouseHandler.X, MouseHandler.Y);
				GameObjectFactory.CreateCube(ActiveCamera.Transform.LocalPosition + ActiveCamera.Transform.Forward * 4f, new Vector3(Mathf.Rnd, Mathf.Rnd, Mathf.Rnd)); //
			});
			MouseHandler.OnMouseMove += MouseHandler_OnMouseMove;
			MouseHandler.RegisterButtonDown(MouseButton.Left, () =>
			{
				Vector2 sp = new Vector2(MouseHandler.X, MouseHandler.Y);
				GameObjectFactory.CreateCube(ActiveCamera.Transform.LocalPosition + ActiveCamera.ScreenToDirection(sp) * 2f, new Vector3(0.3f, 0.3f, 0.3f)).Component<MeshRenderer>().Parameters.SetParameter<float>("_color", Mathf.Rnd, Mathf.Rnd, Mathf.Rnd, 1); //
			});
			//.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", 1, 0, 0, 1);
			GameObjectFactory.CreatePlane(new Vector3(15, 15, 15), Vector3.Zero);
			GameObjectFactory.CreateCube(Vector3.Zero, Vector3.One).Component<MeshRenderer>().Parameters.SetParameter<float>("_color", 1, 0, 0, 1);
			ActiveCamera.TransAccel = 10f;
			postEffect = new Material(Shaders["screen"], RenderMode.Opaque);
			postEffect.AddTexture("_tex", new Surface(Window.Width, Window.Height, SurfaceFormat.Surface2D));
			var sf = SurfaceFormat.Surface2D;
			sf.Multisampling = 4;
			multisampler = new Surface(Window.Width, Window.Height, sf);
			Log.ShowDebug = true;
			Log.Debug("Creating screen buffer");
			//ActiveCamera.SetMultisampling(2);
			//ActiveCamera.SetCameraShader(Shaders["screenCA"],4);
			SceneRenderer.UseAlphaToCoverage = true;
			//ActiveCamera.CameraMaterial.Parameters.SetParameter<float>("baseBlur", 0.1f);
			//ActiveCamera.CameraMaterial.Parameters.SetParameter<float>("blur", 8f);
			//ActiveCamera.CameraMaterial.Parameters.SetParameter<float>("chromatic", 0.03f);
			Log.Debug("Createing gui");
			var guiObj = CreateGameObject("GUI");
			gui = guiObj.AddComponent<Gui>();
			gui.Setup(Window.Width, Window.Height);
			Vector2 ss = ActiveCamera.NearplaneSize;
			guiObj.Transform.LocalPosition = new Vector3(ss.X/-2,ss.Y/2,-(ActiveCamera.ZNear + 0.001f));
			gui.Transform.LocalScale = new Vector3(ss.X, 1, ss.Y);
			guiObj.Transform.Rotate(guiObj.Transform.Right, Mathf.Deg2Rad(90));
			Surface sun = new Surface("sun.png");
			gui.DrawSurface(sun, 0, 0, 1024, 1024);
			defaultFont = new Font("arial", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ß!\"§$%&/()=?`+#*'äüö-.,:; ", 68);
			CameraContainer.AddChild(guiObj);
			rotator = GameObjectFactory.CreateCube(new Vector3(2, 0, 2), Vector3.One);
			rotator.AddChild(GameObjectFactory.CreateCube(new Vector3(2, 0, 2), Vector3.One));
			ActiveCamera.TransAccel *= 0.7f;
			GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 1, 0)).Component<MeshRenderer>().SetMaterial(new Material(Shaders["unlit"], RenderMode.Translucent)).AddTexture("_tex", sun);
			canvas = new Canvas(1024,1024, false);
			canvas.Clear(0, 0, 0, 0.5f);
			canvas.DrawText(",.-+ A B C ABC def 123" , Shaders["text"], defaultFont, 0.5f, new Vector2(300, 300),new Vector4(1,0,0,1));
			planeRenderer = GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 2, 3)).Component<MeshRenderer>();
			planeRenderer.SetMaterial(new Material(Shaders["unlit"], RenderMode.Opaque)).AddTexture("_tex", canvas.Surface);
			
		}
		public override void OnUpdate()
		{
			rotator.Transform.Rotate(Vector3.UnitX, 0.1f);
			gui.Material.Textures["_tex"].Clear();
			Vector2 p = ActiveCamera.WorldToScreen(new Vector3(0,0,0));
			gui.DrawText("Time: " + Time, defaultFont, 0.6f, p, new Vector4(1,0.5f,0.5f,1));
			gui.DrawText("FPS:  " + Fps, defaultFont, 0.6f, new Vector2(p.X, p.Y + 100), new Vector4(1, 0.5f, 0.5f, 1));
			
		}
		void MouseHandler_OnMouseMove(Vector2 position, Vector2 delta)
		{
			float hRot = (delta.X / Window.Width) * 18f;
			float vRot = (delta.Y / Window.Width) * 18f;

			ActiveCamera.Rotate(Vector3.UnitY, hRot*1);
			ActiveCamera.Rotate(ActiveCamera.Transform.Right, vRot*1);
		}
		public override void OnDraw(float time)
		{
			SceneRenderer.RenderMultisampled(ActiveCamera, multisampler, time);
			multisampler.CloneTo(postEffect.Textures["_tex"]);
			canvas.Clear();
			canvas.DrawMaterial(postEffect);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Viewport(0, 0, Window.Width, Window.Height);
			postEffect.Use();
			Helper.DrawScreenQuad();
			GL.UseProgram(0);
			
			//canvas.DrawSurface(postEffect.Textures["_tex"], 0, 0, canvas.Width, canvas.Height);
		}
	}
}
