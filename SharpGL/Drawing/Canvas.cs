using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Drawing;
using SharpGL.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
	public class Canvas
	{
		public Surface Surface {get; private set;}

		public int Width
		{
			get
			{
				return Surface.Width;
			}
		}
		public int Height
		{
			get
			{
				return Surface.Height;
			}
		}
		public Canvas(int width, int height, bool transparency)
		{
			Surface = new Surface(width, height, transparency ? SurfaceFormat.Surface2DAlpha : SurfaceFormat.Surface2D);
		}
		public void Clear(float r = 0, float g = 0, float b = 0, float a = 0)
		{
			Surface.Clear(r, g, b, a);
		}
		public void DrawSurface(Surface surface, int dstX, int dstY, int dstW, int dstH)
		{
			surface.CloneTo(this.Surface, dstX, dstY, dstW, dstH);
			//Modified = true;
		}
		public void DrawTexture(Texture2D texture, int dstX, int dstY, int dstW, int dstH)
		{
			int fbo = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
			GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, fbo, 0);
			Surface.BindFramebuffer(FramebufferTarget.DrawFramebuffer);
			GL.BlitFramebuffer(0, 0, texture.Width, texture.Height, dstX, dstY, dstW, dstH, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
		}
		public void DrawMaterial(Material material)
		{
			GL.Viewport(0, 0, Surface.Width, Surface.Height);
			
			Surface.BindFramebuffer();
			material.Use();
			Helper.DrawScreenQuad();
			GL.UseProgram(0);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
		public void DrawText(string text, Shader shader, Font font, float charDistance, Vector2 position, Vector4 color)
		{
			font.DrawString(Surface, shader, text, charDistance, position, color);
			//Modified = true;
		}
		public void DrawText(string text, Shader shader, Font font, float charDistance, Vector2 position)
		{
			font.DrawString(Surface, shader, text, charDistance, position, Vector4.One);
			//Modified = true;
		}
		
	}
}
