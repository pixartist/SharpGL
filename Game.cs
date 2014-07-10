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
namespace SharpGL
{
    public abstract class Game
    {
        protected DrawContext DrawContext { get; private set; }
        protected Color BackgroundColor { get; set; }
        protected Surface BackgroundImage
        {
            get
            {
                return DrawContext.Background;
            }
            set
            {
                DrawContext.Background = value;
            }
        }
        public static GameWindow Window { get; private set; }
        
        public Game(int width, int height)
        {
            Window = new GameWindow(width, height);
            Window.Load += OnLoadInternal;
            Window.Resize += OnResizeInternal;
            Window.UpdateFrame += OnUpdateInternal;
            Window.RenderFrame += OnRenderInternal;
            Window.Run(60.0);
        }
        
        private void OnRenderInternal(object sender, FrameEventArgs e)
        {
            
            var window = (GameWindow)sender;
            DrawContext.Begin(BackgroundColor);
            OnDraw();
            DrawContext.End();
            window.SwapBuffers();
        }

        private void OnUpdateInternal(object sender, FrameEventArgs e)
        {
            KeyboardHandler.Update();
            OnUpdate();
        }

        private void OnResizeInternal(object sender, EventArgs e)
        {
            var window = (GameWindow)sender;
            DrawContext.SetScreenSize(window.Width, window.Height);
        }

        private void OnLoadInternal(object sender, EventArgs e)
        {
            var window = (GameWindow)sender;
            KeyboardHandler.Init(window);
            EnabledTextureBlending();
            CreateDrawBuffer(window.Width, window.Height);
            OnLoad();
        }
        private void CreateDrawBuffer(int width, int height)
        {
            DrawContext = new DrawContext(width, height);
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
