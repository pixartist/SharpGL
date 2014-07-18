using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using SharpGL.Drawing;
using SharpGL.Input;
using SharpGL.Components;
using SharpGL.Factories;
namespace SharpGL
{
	public class App
	{
		public Dictionary<string, Shader> Shaders;
		public Dictionary<string, Material> Materials;
		public GameObjectFactory GameObjectFactory { get; protected set; }
		public PrimitiveFactory PrimitiveFactory { get; protected set; }
		public MeshRenderCore MeshRenderCore { get; protected set; }
		private Dictionary<string, GameObject> GameObjects;
		
		private System.Diagnostics.Stopwatch stopWatch;
		private System.Diagnostics.Stopwatch time;
		public float Time
		{
			get
			{
				return time.ElapsedMilliseconds / 1000f;
			}
		}
		public  GameWindow Window { get; private set; }
        protected Color BackgroundColor { get; set; }


		public GameObject CameraContainer { get; set; }
		public Camera ActiveCamera { get; set; }

		public float DT { get; private set; }
        public App(int width, int height)
        {
			GameObjects = new Dictionary<string, GameObject>();
			Shaders = new Dictionary<string, Shader>();
			Materials = new Dictionary<string, Material>();

            Window = new GameWindow(width, height, new GraphicsMode(32, 24,0, 4));
            Window.Load += OnLoadInternal;
            Window.Resize += OnResizeInternal;
            Window.UpdateFrame += OnUpdateInternal;
            Window.RenderFrame += OnRenderInternal;
			MeshRenderCore = new SharpGL.MeshRenderCore();
			SetupGL();
			
			CameraContainer = CreateGameObject("Camera");
			ActiveCamera = CameraContainer.AddComponent<Camera>();
			ActiveCamera.TransAccel = 4f;
			stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			time = new System.Diagnostics.Stopwatch();
			time.Start();

			Shaders.Add("unlit", new Shader("Shaders/unlit.glsl", "vertex", null, "fragment"));
			Shaders.Add("screen", new Shader("Shaders/screen.glsl", "vertex", null, "fragment"));
			Shaders.Add("screenCA", new Shader("Shaders/chromaticAbberation.glsl", "vertex", null, "fragment"));

			Materials.Add("unlit", new Material(Shaders["unlit"]));

			GameObjectFactory = new GameObjectFactory(this);
			PrimitiveFactory = new PrimitiveFactory();
			Window.Run(60.0);
			
        }
        public GameObject CreateGameObject(string name)
		{
			int  i = 2;
			string newName = name;
			while(GameObjects.ContainsKey(newName))
			{
				newName = name + i;
				i++;
			}
			var go = new GameObject(newName, this);
			GameObjects.Add(newName, go);
			return go;
		}
		
        private void OnRenderInternal(object sender, FrameEventArgs e)
        {
            var window = (GameWindow)sender;
			
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			if(ActiveCamera != null)
				ActiveCamera.BeginDraw();
			/*foreach(var go in GameObjects.Values)
			{
				go.Render(Time);
			}*/
			MeshRenderCore.Render(ActiveCamera, Time);
			//User drawing
            OnDraw();
			if (ActiveCamera != null)
				ActiveCamera.EndDraw();
            window.SwapBuffers();
			
        }
		
		
        private void OnUpdateInternal(object sender, FrameEventArgs e)
        {
			DT = stopWatch.ElapsedMilliseconds / 1000.0f;
			stopWatch.Restart();
			ActiveCamera.Update(DT);
			MouseHandler.Update();
            KeyboardHandler.Update();
            OnUpdate();
        }

        private void OnResizeInternal(object sender, EventArgs e)
        {
            var window = (GameWindow)sender;
        }

        private void OnLoadInternal(object sender, EventArgs e)
        {
            var window = (GameWindow)sender;
            KeyboardHandler.Init(window);
			MouseHandler.Init(window);
            OnLoad();
        }
		private void SetupGL()
		{
			GL.Enable(EnableCap.CullFace);
			EnabledTextureBlending();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			GL.DepthFunc(DepthFunction.Less);
			GL.DepthRange(0.0f, 1.0f);
		}
		private void EnabledTextureBlending()
		{
			GL.Enable(EnableCap.Texture2D);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend);
		}
        
        public virtual void OnLoad() { }
        public virtual void OnDraw() { }
        public virtual void OnUpdate() { }

    }
}
