using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
namespace SharpGL.Drawing
{
    public class DrawContext
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Material Material {get; private set;}
        public Surface Background { get; set; }
        private int vpX, vpY, vpW, vpH;
        public float AspectRatio { get; private set; }
        private List<Sprite> spritesToDraw;
        public int Width
        {
            get
            {
                return Material.Textures[0].Surface.Width;
            }
        }
        public int Height
        {
            get
            {
                return Material.Textures[0].Surface.Height;
            }
        }
        public DrawContext(int width, int height)
        {
            var surface = new Surface();
            surface.Create(width, height, new Surface.SurfaceFormat { WrapMode = TextureWrapMode.ClampToEdge, DepthBuffer = true });
            AspectRatio = width / (float)height;
            this.Material = new Material(surface);
            Background = null;
            X = 0;
            Y = 0;
            spritesToDraw = new List<Sprite>();
        }
        public void SetScreenSize(int width, int height)
        {
            float newAspect = width / (float)height;
            vpX = 0;
            vpY = 0;
            if(newAspect > AspectRatio)
            {
                vpH = height;
                vpW = (int)Math.Round(vpH * AspectRatio);
                vpX = (width - vpW) / 2;
            }
            else
            {
                vpW = width;
                vpH = (int)Math.Round(vpW/AspectRatio);
                vpY = (height - vpH) / 2;
            }
            
        }
        public void Translate(float x, float y)
        {
            X += x;
            Y += y;
        }
        internal void Begin(Color background)
        {
            
            GL.UseProgram(0);
            GL.Viewport(0, 0, Width, Height);
            Material.Textures[0].Surface.BindFramebuffer();
            
            GL.ClearColor(background);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0.0, 0.0, 1.0);
            
            if(Background != null)
            {
                Background.BindTexture();
                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex3(0, 0, 0);
                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex3(0, Height, 0);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(Width, Height, 0);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex3(Width, 0, 0);
                GL.End();
            }
        }
        public void Draw(Sprite sprite)
        {
            float left = sprite.position.X - sprite.size.X * sprite.scale * 0.5f;
            float right = sprite.position.X + sprite.size.X * sprite.scale * 0.5f;
            float top = sprite.position.Y - sprite.size.Y * sprite.scale * 0.5f;
            float bottom = sprite.position.Y + sprite.size.Y * sprite.scale * 0.5f;
            float xAdd = Width / 2.0f;
            float yAdd = Height / 2.0f;
            if(left < X + Width - xAdd && right > X-xAdd && top < Y + Height-yAdd && bottom > Y - yAdd)
            {
                spritesToDraw.Add(sprite);
                
            }
        }

        internal void End()
        {
            spritesToDraw.Sort((a,b) => {return (a.z >= b.z ? 1 : -1);});
            float xAdd = Width / 2.0f;
            float yAdd = Height / 2.0f;
            foreach(var s in spritesToDraw)
            {
                s.Draw(-(X-xAdd), -(Y-yAdd));
            }
            spritesToDraw.Clear();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(vpX, vpY, vpW, vpH);
			//GL.LoadIdentity();
			//GL.Ortho(-10000, 1, 0, 1, -10, 10);
			//ErrorCode c = GL.GetError();
			//GL.MatrixMode(MatrixMode.Modelview);
			//c = GL.GetError();
			
            
            Material.Use();
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f);
			GL.Vertex3(-1, -1, 0);
            GL.TexCoord2(0.0f, 0.0f);
			GL.Vertex3(-1, 1, 0);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(1, 1, 0);
            GL.TexCoord2(1.0f, 1.0f);
			GL.Vertex3(1, -1, 0);

            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
