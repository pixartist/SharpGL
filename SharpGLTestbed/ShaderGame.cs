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
			//handle movement
			float camSpeed = 0.05f;
			KeyboardHandler.RegisterKeyDown(Key.W, () => { ActiveCamera.MoveForward(camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.S, () => { ActiveCamera.MoveForward(-camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.A, () => { ActiveCamera.MoveRight(-camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.D, () => { ActiveCamera.MoveRight(camSpeed); });
			KeyboardHandler.RegisterKeyDown(Key.ShiftLeft, () => { ActiveCamera.Translate(new Vector3(0, -camSpeed, 0)); });
			KeyboardHandler.RegisterKeyDown(Key.Space, () => { ActiveCamera.Translate(new Vector3(0, camSpeed, 0)); });
			KeyboardHandler.RegisterKeyDown(Key.Escape, () => { Window.Close();});
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
				var go = GameObjectFactory.CreateCube(ActiveCamera.Transform.LocalPosition + ActiveCamera.ScreenToDirection(sp) * 2f, new Vector3(0.3f, 0.3f, 0.3f)); //
				go.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", Mathf.Rnd, Mathf.Rnd, Mathf.Rnd, 1);
				go.AddComponent<Rigidbody>().SetCollisionBox(Vector3.One * 0.3f, Vector3.One * 0.15f);
			});

			Log.ShowDebug = true;
			Log.Debug("Creating gui");

			//Setup font
			defaultFont = new Font("arial", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ß!\"§$%&/()=?`+#*'äüö-.,:; ", 68);
			//Setup Camera
			ActiveCamera.TransAccel = 10f;
			//Setup Multisampler & Screen buffer
			var sf = SurfaceFormat.Surface2D;
			sf.Multisampling = 4;
			multisampler = new Surface(Window.Width, Window.Height, sf);

			postEffect = new Material(Shaders["screenCA"], RenderMode.Opaque);
			sf = SurfaceFormat.Surface2D;
			sf.WrapMode = TextureWrapMode.Clamp;
			postEffect.AddTexture("_tex", new Surface(Window.Width, Window.Height, sf));
			postEffect.Parameters.SetParameter<float>("baseBlur", 0f);
			postEffect.Parameters.SetParameter<float>("blur", 2f);
			postEffect.Parameters.SetParameter<float>("chromatic", 0.1f);

			//Create base plate
			var prb = GameObjectFactory.CreatePlane(new Vector3(15, 15, 15), Vector3.Zero).AddComponent<Rigidbody>();
			prb.SetCollisionBox(new Vector3(15,0.02f,15), new Vector3(7.5f, -0.01f, 7.5f));
			prb.Body.IsStatic = true;
			//Create sun image plane
			Surface sun = new Surface("sun.png");
			(GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 1, 0)).Component<MeshRenderer>().Material = new Material(Shaders["unlit"], RenderMode.Translucent)).AddTexture("_tex", sun);
			//red cube
			var cu = GameObjectFactory.CreateCube(Vector3.Zero, Vector3.One);
			cu.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", 1, 0, 0, 1);
			prb = cu.AddComponent<Rigidbody>();
			prb.SetCollisionConvexMesh(cu.Component<MeshRenderer>().Mesh, cu.Transform.Scale, new Vector3(0.5f, 0.5f, 0.5f));
			prb.Body.IsStatic = true;
			//Create gui
			var guiObj = CreateGameObject("GUI");
			gui = guiObj.AddComponent<Gui>();
			gui.Setup(Window.Width, Window.Height);
			Vector2 ss = ActiveCamera.NearplaneSize;
			guiObj.Transform.LocalPosition = new Vector3(ss.X/-2,ss.Y/2,-(ActiveCamera.ZNear + 0.001f));
			gui.Transform.LocalScale = new Vector3(ss.X, 1, ss.Y);
			guiObj.Transform.Rotate(guiObj.Transform.Right, Mathf.Deg2Rad(90));
			CameraContainer.AddChild(guiObj);

			
			
			//setup rotating cubes
			rotator = GameObjectFactory.CreateCube(new Vector3(2, 0, 2), Vector3.One);
			rotator.AddChild(GameObjectFactory.CreateCube(new Vector3(2, 0, 2), Vector3.One));
			
			//Create plane for camera image displaying
			canvas = new Canvas(1024,1024, false);
			canvas.Clear(0, 0, 0, 0.5f);
			canvas.DrawText(",.-+ A B C ABC def 123" , Shaders["text"], defaultFont, 0.5f, new Vector2(300, 300),new Vector4(1,0,0,1));
			planeRenderer = GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 2, 3)).Component<MeshRenderer>();
			(planeRenderer.Material = new Material(Shaders["unlit"], RenderMode.Opaque)).AddTexture("_tex", canvas.Surface);

			//Gol shader
			var golObject = GameObjectFactory.CreatePlane(Vector3.One * 5, new Vector3(8, 3, 8));
			var goldR = golObject.Component<MeshRenderer>().Material = new Material(Shaders["gol"], RenderMode.Opaque);
			var surf = SurfaceFormat.Surface2D;
			surf.DepthBuffer = false;
			surf.MipMapping = true;
			golObject.Component<MeshRenderer>().Material.AddTexture("_tex", new Surface(1024, 1024, surf));
			Log.Write(GL.GetError().ToString());

			//physics 
			var vcube = GameObjectFactory.CreateCube(new Vector3(-0.3f, 2, -0.3f), Vector3.One * 2);
			prb = vcube.AddComponent<Rigidbody>();
			prb.SetCollisionConvexMesh(vcube.Component<MeshRenderer>().Mesh, vcube.Transform.Scale, new Vector3(1f, 1f, 1f));
			
		}


		public override void OnUpdate()
		{
			//rotate my cubes
			rotator.Transform.Rotate(Vector3.UnitX, 0.1f);
			
			
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
			//Render gui text
			gui.Material.Textures["_tex"].Clear();
			Vector2 p = ActiveCamera.WorldToScreen(new Vector3(0, 0, 0));
			gui.DrawText("Time: " + time, defaultFont, 0.6f, p, new Vector4(1, 0.5f, 0.5f, 1));
			gui.DrawText("FPS:  " + Fps, defaultFont, 0.6f, new Vector2(p.X, p.Y + 100), new Vector4(1, 0.5f, 0.5f, 1));
			
			//Render multisampler
			SceneRenderer.RenderMultisampled(ActiveCamera, multisampler, time);
			multisampler.CloneTo(postEffect.Textures["_tex"]);

			canvas.Clear();
			canvas.DrawMaterial(postEffect);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Viewport(0, 0, Window.Width, Window.Height);
			postEffect.Use();
			Helper.DrawScreenQuad();
			GL.UseProgram(0);
			 
			//SceneRenderer.Render(ActiveCamera, time);
			//canvas.DrawSurface(postEffect.Textures["_tex"], 0, 0, canvas.Width, canvas.Height);
		}
	}
}
