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
using SharpGL.Components.BulletPhysics;
using System.Threading;
namespace ModernShaders
{
	class ShaderGame : App
	{
		private GameObject rotator;
        private GameObject car;
		private Gui gui;
		private Font defaultFont;
		private Material postEffect;
		private Surface multisampler;
		private MeshRenderer planeRenderer;
		private Canvas canvas;
		private PlayerControllerGhost playerController;
        private Random rnd;
        private Surface sun;
		public ShaderGame(int width, int height) : base(width, height)
		{
			
		}
		public override void OnLoad()
		{
			//handle movement
			rnd = new Random();
			KeyboardHandler.RegisterKeyDown(Key.W, () => { playerController.MoveForward(); });
			KeyboardHandler.RegisterKeyDown(Key.S, () => { playerController.MoveBack(); });
			KeyboardHandler.RegisterKeyDown(Key.A, () => { playerController.MoveLeft(); });
			KeyboardHandler.RegisterKeyDown(Key.D, () => { playerController.MoveRight(); });
			KeyboardHandler.RegisterKeyDown(Key.ShiftLeft, () => {  /*playerController.Translate(-Vector3.UnitY);*/ });
            KeyboardHandler.RegisterKeyDown(Key.Space, () => { /*playerController.Jump(); */playerController.Translate(Vector3.UnitY); });
			KeyboardHandler.RegisterKeyDown(Key.Escape, () => { Window.Close();});
            MouseHandler.MouseLocked = true;
            MouseHandler.CursorVisible = false;
			//KeyboardHandler.RegisterKeyDown(Key.Q, () => { ActiveCamera.Transform.LocalRotation = ActiveCamera.Transform.LocalPosition.LookAt(Vector3.Zero, Vector3.UnitY); });
			KeyboardHandler.RegisterKeyDown(Key.E, () =>
			{
				Vector2 sp = new Vector2(MouseHandler.X, MouseHandler.Y);
				GameObjectFactory.CreateCube(ActiveCamera.Transform.LocalPosition + ActiveCamera.Transform.LocalForward * 4f, new Vector3(Mathf.Rnd, Mathf.Rnd, Mathf.Rnd)); //
                
			});
			MouseHandler.OnMouseMove += MouseHandler_OnMouseMove;
			MouseHandler.RegisterButtonDown(MouseButton.Left, () =>
			{
				Vector2 sp = new Vector2(MouseHandler.X, MouseHandler.Y);
				var go = GameObjectFactory.CreateCube(ActiveCamera.Transform.LocalPosition + ActiveCamera.ScreenToDirection(sp) * 2f, new Vector3(0.3f, 0.3f, 0.3f)); //
				go.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", Mathf.Rnd, Mathf.Rnd, Mathf.Rnd, 1);
				go.AddComponent<Rigidbody>().SetCollisionBox(Vector3.One * 0.15f, Vector3.One * 0.3f);
			});

			Log.ShowDebug = true;
			Log.Debug("Creating gui");
            SceneRenderer.AmbientLight = new Vector3(0.05f, 0.05f, 0.05f);
            SceneRenderer.SkylightColor = new Vector3(1, 1, 1);
            SceneRenderer.SkylightDirection = new Vector3(1, 0, 0);
			//Setup font
			defaultFont = new Font("arial", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ß!\"§$%&/()=?`+#*'äüö-.,:; ", 68);
			//Setup Camera
            
			ActiveCamera.GameObject.Transform.Position = new Vector3(4, 3, 4);
            playerController = ActiveCamera.GameObject.AddComponent<PlayerControllerGhost>();
			//Setup Multisampler & Screen buffer
			var sf = SurfaceFormat.Surface2D;
			sf.Multisampling = 1;
			multisampler = new Surface(Window.Width, Window.Height, sf);

			postEffect = new Material(Shaders["screenCA"], RenderMode.Opaque);
			sf = SurfaceFormat.Surface2D;
			sf.WrapMode = TextureWrapMode.MirroredRepeat;
			postEffect.AddTexture("_tex", new Surface(Window.Width, Window.Height, sf));
			postEffect.Parameters.SetParameter<float>("baseBlur", 0f);
			postEffect.Parameters.SetParameter<float>("blur", 2f);
			postEffect.Parameters.SetParameter<float>("chromatic", 0.1f);

			//Create base plate
            var basePlate = GameObjectFactory.CreatePlane(new Vector3(15, 15, 15), Vector3.Zero);
            basePlate.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
            var prb = basePlate.AddComponent<Rigidbody>();
            prb.SetCollisionBox(new Vector3(7.5f, -0.25f, 7.5f), new Vector3(15, 1f, 15));
            prb.MakeStatic();
			//Create sun image plane
			sun = new Surface("sun.png");
            Material sunMat = new Material(Shaders["lit"], RenderMode.Translucent);
            sunMat.AddTexture("_tex", sun);
            sunMat.Parameters.SetParameter<float>("_color", 1f, 1f, 0f, 1f);
            GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 2, 0)).Component<MeshRenderer>().Material = sunMat ;
			//red cube
			var cu = GameObjectFactory.CreateCube(new Vector3(0,3.5f,0), Vector3.One*2f);
			cu.Component<MeshRenderer>().Parameters.SetParameter<float>("_color", 1, 0, 0, 1);
            //prb = cu.AddComponent<Rigidbody>();
            //prb.SetCollisionConvexMesh(cu.Component<MeshRenderer>().Mesh, Vector3.One * 0.5f, cu.Transform.Scale);
            //prb.SetMass(8.0f);
			//prb.Body.IsStatic = true;
			//Create gui
			var guiObj = CreateGameObject("GUI");
			gui = guiObj.AddComponent<Gui>();
			gui.Setup(Window.Width, Window.Height);
			Vector2 ss = ActiveCamera.NearplaneSize;
			guiObj.Transform.LocalPosition = new Vector3(ss.X/-2,ss.Y/2,-(ActiveCamera.ZNear + 0.001f));
			gui.Transform.LocalScale = new Vector3(ss.X, 1, ss.Y);
			guiObj.Transform.Rotate(guiObj.Transform.LocalRight, Mathf.Deg2Rad(90));
            guiObj.Parent = CameraContainer;
            //Mesh p = Mesh.LoadOBJ("E:\\Coding\\WIP\\SharpGL\\Models\\BumbleBee\\RB-BumbleBee.obj");
           /* var robot = CreateGameObject("car");
            robot.Transform.LocalScale = new Vector3(0.005f, 0.005f, 0.005f);
            robot.Transform.Rotate(Vector3.UnitX,- Mathf.PI/2);
            robot.Transform.Translate(new Vector3(-2f, 0.4f, 0));
            var robotM = robot.AddComponent<MeshRenderer>();
            robotM.Mesh = p;
            robotM.Material = Materials["lit"];
            robotM.Parameters.SetParameter<float>("_color", 0.3f, 0.3f, 0.4f, 1f);*/
            Mesh p = Mesh.LoadOBJ("E:\\Coding\\WIP\\SharpGL\\Models\\911\\Porsche_911_GT2.obj");
            car = CreateGameObject("car");
            //robot.Transform.LocalScale = new Vector3(0.005f, 0.005f, 0.005f);
           // robot.Transform.Rotate(Vector3.UnitX, -Mathf.PI / 2);
            car.Transform.Translate(new Vector3(1f, 1f, 0));
            var carM = car.AddComponent<MeshRenderer>();
            carM.Mesh = p;
            carM.Material = Materials["lit"];
            carM.Parameters.SetParameter<float>("_color", 0.6f, 0.3f, 0.4f, 1f);
			//setup rotating cubes
			rotator = GameObjectFactory.CreateCube(new Vector3(-4, 0, 2), Vector3.One);
            GameObjectFactory.CreateCube(new Vector3(2, 0, 2), Vector3.One).Parent = rotator;
			
			//Create plane for camera image displaying
			canvas = new Canvas(1024,1024, false);
			canvas.Clear(0, 0, 0, 0.5f);
			canvas.DrawText(",.-+ A B C ABC def 123" , Shaders["text"], defaultFont, 0.5f, new Vector2(300, 300),new Vector4(1,0,0,1));
            var plane = GameObjectFactory.CreatePlane(Vector3.One * 4, new Vector3(3, 4, -2));
            plane.Transform.Rotate(Vector3.UnitX, 1f);
			planeRenderer = plane.Component<MeshRenderer>();
			(planeRenderer.Material = new Material(Shaders["unlit"], RenderMode.Opaque)).AddTexture("_tex", canvas.Surface);
			
		}


		public override void OnUpdate()
		{
			//rotate my cubes
			rotator.Transform.Rotate(Vector3.UnitX, 0.03f);
            car.Transform.Rotate(Vector3.UnitY, 0.01f);
			
		}
		void MouseHandler_OnMouseMove(Vector2 position, Vector2 delta)
		{
			float hRot = (delta.X / Window.Width) * 18f;
			float vRot = (delta.Y / Window.Width) * 18f;

			ActiveCamera.Rotate(Vector3.UnitY, hRot*1);
			ActiveCamera.Rotate(ActiveCamera.Transform.LocalRight, vRot*1);
		}
		public override void OnDraw(float time)
		{
			//Render gui text
			gui.Material.Textures["_tex"].Clear();
			Vector2 p = ActiveCamera.WorldToScreen(new Vector3(10, 0, 10));
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

		}
	}
}
