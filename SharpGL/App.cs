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
		/// <summary>
		/// A dictionary to store shaders (contains default shaders, list dictionary keys for the names)
		/// </summary>
		public Dictionary<string, Shader> Shaders;
		/// <summary>
		/// A dictionary to store materials (contains default materials, list dictionary keys for the names)
		/// </summary>
		public Dictionary<string, Material> Materials;
		/// <summary>
		/// Factory to create basic game objects
		/// </summary>
		public GameObjectFactory GameObjectFactory { get; protected set; }
		/// <summary>
		/// Factory to create / get primitive meshes
		/// </summary>
		public PrimitiveFactory PrimitiveFactory { get; protected set; }
		/// <summary>
		/// Rendering core. Renders registered MeshRenderers grouped by Mesh -> Material
		/// </summary>
		public Jitter.World PhysicsWorld { get; protected set; }
		public Vector3 WorldSize { get; set; }
		public SceneRenderer SceneRenderer { get; protected set; }

		public BlendingFactorSrc DefaultBlendFactorSrc { get; set; }
		public BlendingFactorDest DefaultBlendFactorDest { get; set; }
		public float Fps { get; private set; }
		public GameObject SceneRoot { get; private set; }
		private List<DestructableObject> destroyed;
		private System.Diagnostics.Stopwatch stopWatch;
		private System.Diagnostics.Stopwatch time;
		private long lastTime;
		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		public float Time
		{
			get
			{
				return time.ElapsedMilliseconds / 1000f;
			}
		}
		/// <summary>
		/// OpenTK Window instance
		/// </summary>
		public  GameWindow Window { get; private set; }
		/// <summary>
		/// Window background color
		/// </summary>
        protected Color BackgroundColor { get; set; }

		/// <summary>
		/// returns the Gameobject of the ActiveCamera
		/// </summary>
		public GameObject CameraContainer
		{
			get
			{
				if (ActiveCamera == null)
					return null;
				return ActiveCamera.GameObject;
			}
		}
		/// <summary>
		/// Stores the active camera. By default this camera is passed to the MeshRendererCore
		/// </summary>
		public Camera ActiveCamera { get; set; }
		/// <summary>
		/// Delta time of the current update call
		/// </summary>
		public float DT { get; private set; }
        public App(int width, int height)
        {
			WorldSize = new Vector3(20, 20, 20);
			lastTime = 0;
			SceneRoot = new GameObject("SceneRoot", this);
			Shaders = new Dictionary<string, Shader>();
			Materials = new Dictionary<string, Material>();
			destroyed = new List<DestructableObject>();
			DefaultBlendFactorSrc = BlendingFactorSrc.SrcAlpha;
			DefaultBlendFactorDest = BlendingFactorDest.OneMinusSrcAlpha;
			
            Window = new GameWindow(width, height, new GraphicsMode(32, 24,0, 0));
            Window.Load += OnLoadInternal;
            Window.Resize += OnResizeInternal;
            Window.UpdateFrame += OnUpdateInternal;
            Window.RenderFrame += OnRenderInternal;
			SceneRenderer = new SceneRenderer(this);
			SetupGL();
			
			var cameraContainer = CreateGameObject("Camera");
			ActiveCamera = cameraContainer.AddComponent<Camera>();
			ActiveCamera.TransAccel = 4f;
			stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			time = new System.Diagnostics.Stopwatch();
			time.Start();

			Shaders.Add("unlit", new Shader("Shaders/unlit.glsl", "vertex", null, "fragment"));
			Shaders.Add("screen", new Shader("Shaders/screen.glsl", "vertex", null, "fragment"));
			Shaders.Add("screenCA", new Shader("Shaders/chromaticAbberation.glsl", "vertex", null, "fragment"));
			Shaders.Add("text", new Shader("Shaders/text.glsl", "vertex", null, "fragment"));
			Shaders.Add("gol", new Shader("Shaders/gol.glsl", "vertex", null, "fragment"));
			Materials.Add("unlit", new Material(Shaders["unlit"], RenderMode.Opaque));

			GameObjectFactory = new GameObjectFactory(this);
			PrimitiveFactory = new PrimitiveFactory();

			PhysicsWorld = new Jitter.World(new Jitter.Collision.CollisionSystemSAP());
			PhysicsWorld.Gravity = new Jitter.LinearMath.JVector(0, -9.81f, 0);


			Window.Run(60.0);
			
        }
		/// <summary>
		/// Creates a new empty game object
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public GameObject CreateGameObject(string name)
		{
			return SceneRoot.AddChild(new GameObject(name, this));
		}
		public void DestroyGameObject(GameObject gameObject)
		{
			destroyed.Add(gameObject);
		}
        private void OnRenderInternal(object sender, FrameEventArgs e)
        {
			long newTime = time.ElapsedMilliseconds;
			long delta = (newTime - lastTime);
			if(delta > 0)
				Fps = Fps * 0.9f + (1000f / delta)*0.1f;
			lastTime = newTime;
            var window = (GameWindow)sender;
			GL.Viewport(0, 0, window.Width, window.Height);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			OnDraw(Time);
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
			if (PhysicsWorld != null)
				PhysicsWorld.Step(DT, true);
			SceneRoot.Update(DT);
            
			foreach(var c in destroyed)
			{
				if(c != SceneRoot)
					c.DestroyInternal();
			}
			destroyed.Clear();
        }
		internal void ScheduleDestruction(DestructableObject obj)
		{
			destroyed.Add(obj);
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
			ResetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			GL.DepthFunc(DepthFunction.Less);
			GL.DepthRange(0.0f, 1.0f);
			GL.Enable(EnableCap.Texture2D);
		}
		public void ResetBlendFunc()
		{
			GL.BlendFunc(DefaultBlendFactorSrc, DefaultBlendFactorDest);
			GL.Enable(EnableCap.Blend);
		}
        /// <summary>
        /// Called when the game has finished initializing
        /// </summary>
        public virtual void OnLoad() { }
		/// <summary>
		/// Called every update
		/// </summary>
        public virtual void OnUpdate() { }
		public virtual void OnDraw(float time) 
		{

			SceneRenderer.Render(ActiveCamera, Time);
		}
    }
}
