using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
		/// The Physics engine instance. Use this to interact with physics objects
		/// </summary>
		public BulletSharp.DiscreteDynamicsWorld PhysicsWorld { get; protected set; }
        /// <summary>
        /// The size of the scene. All objects outside these boundries will be destroyed.
        /// </summary>
		public Vector3 WorldSize { get; set; }
        /// <summary>
        /// Rendering core. Renders registered MeshRenderers grouped by Mesh -> Material
        /// </summary>
		public SceneRenderer SceneRenderer { get; protected set; }
        /// <summary>
        /// The default source factor for blending
        /// </summary>
		public BlendingFactorSrc DefaultBlendFactorSrc { get; set; }
        /// <summary>
        /// The default destination factor for blending
        /// </summary>
		public BlendingFactorDest DefaultBlendFactorDest { get; set; }
        /// <summary>
        /// The current frames per second
        /// </summary>
		public float Fps { get; private set; }
        /// <summary>
        /// The root object of the scene. All GameObjects are stored within this tree hierarchy.
        /// </summary>
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
			WorldSize = new Vector3(200, 200, 200);
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
            Window.Closing += Window_Closing;
            Window.UpdateFrame += OnUpdateInternal;
            Window.RenderFrame += OnRenderInternal;
			SceneRenderer = new SceneRenderer(this);
			SetupGL();
			
			var cameraContainer = CreateGameObject("Camera");
			ActiveCamera = cameraContainer.AddComponent<Camera>();
			stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			time = new System.Diagnostics.Stopwatch();
			time.Start();

			Shaders.Add("unlit", new Shader("Shaders/unlit.glsl", "vertex", null, "fragment"));
			Shaders.Add("screen", new Shader("Shaders/screen.glsl", "vertex", null, "fragment"));
			Shaders.Add("screenCA", new Shader("Shaders/chromaticAbberation.glsl", "vertex", null, "fragment"));
			Shaders.Add("text", new Shader("Shaders/text.glsl", "vertex", null, "fragment"));
			Shaders.Add("gol", new Shader("Shaders/gol.glsl", "vertex", null, "fragment"));
            Shaders.Add("lit", new Shader("Shaders/lit.glsl", "vertex", null, "fragment"));
			Materials.Add("unlit", new Material(Shaders["unlit"], RenderMode.Opaque));
            Materials.Add("lit", new Material(Shaders["lit"], RenderMode.Opaque));
			GameObjectFactory = new GameObjectFactory(this);
			PrimitiveFactory = new PrimitiveFactory();
			//var cs = new Jitter.Collision.CollisionSystemPersistentSAP();
			
			//PhysicsWorld = new Jitter.World(cs);
			
			//PhysicsWorld.Gravity = new Jitter.LinearMath.JVector(0, -9.81f, 0);
            
            var collisionConfig = new BulletSharp.DefaultCollisionConfiguration();
            PhysicsWorld = new BulletSharp.DiscreteDynamicsWorld(
                new BulletSharp.CollisionDispatcher(collisionConfig), 
                new BulletSharp.DbvtBroadphase(), 
                new BulletSharp.SequentialImpulseConstraintSolver(), 
                collisionConfig);
            
            Ray.Init(this);
            
			Window.Run(60.0);
			
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PhysicsWorld != null)
                PhysicsWorld.Dispose();
            OnClosing();
        }
		/// <summary>
		/// Creates a new empty game object
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public GameObject CreateGameObject(string name)
		{
            var go = new GameObject(name, this);
            go.Parent = SceneRoot;
            return go;
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
				Fps = Fps * 0.95f + (1000f / delta)*0.05f;
			lastTime = newTime;
            var window = (GameWindow)sender;
			GL.Viewport(0, 0, window.Width, window.Height);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			OnDraw(Time);
            window.SwapBuffers();
            if (PhysicsWorld != null)
                PhysicsWorld.StepSimulation(DT);
        }
		
		
        private void OnUpdateInternal(object sender, FrameEventArgs e)
        {
			DT = stopWatch.ElapsedMilliseconds / 1000.0f;
			stopWatch.Restart();
			ActiveCamera.Update(DT);
			MouseHandler.Update();
            KeyboardHandler.Update();
			OnUpdate();
            SceneRoot.Update(DT);
			
				//PhysicsWorld.Step(DT, true);
			
            
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
        /// <summary>
        /// Called when the application is closing
        /// </summary>
        public virtual void OnClosing() { }
        /// <summary>
        /// Called when drawing. Calls SceneRenderer.Render() by default
        /// </summary>
        /// <param name="time"></param>
		public virtual void OnDraw(float time) 
		{
			SceneRenderer.Render(ActiveCamera, Time);
		}
    }
}
