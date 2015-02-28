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
    /// <summary>
    /// A Canvas allows for easy rendering of Textures, Surfaces and Text to a surface.
    /// </summary>
	public class Canvas
	{
        /// <summary>
        /// The Surface of the Canvas object
        /// </summary>
		public Surface Surface {get; private set;}
        /// <summary>
        /// The width of the canvas
        /// </summary>
		public int Width
		{
			get
			{
				return Surface.Width;
			}
		}
        /// <summary>
        /// The height of the canvas
        /// </summary>
		public int Height
		{
			get
			{
				return Surface.Height;
			}
		}
        /// <summary>
        /// Creates a canvas object
        /// </summary>
        /// <param name="width">Width of the internal Surface</param>
        /// <param name="height">Height of the internal Surface</param>
        /// <param name="transparency">If true an alpha channel is used</param>
		public Canvas(int width, int height, bool transparency)
		{
			Surface = new Surface(width, height, transparency ? SurfaceFormat.Surface2DAlpha : SurfaceFormat.Surface2D);
		}
        /// <summary>
        /// Clears the Canvas with the provided color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
		public void Clear(float r = 0, float g = 0, float b = 0, float a = 0)
		{
			Surface.Clear(r, g, b, a);
		}
        /// <summary>
        /// Draws a Surface to the Canvas
        /// </summary>
        /// <param name="surface">The Surface to draw</param>
        /// <param name="dstX">X-Location to draw</param>
        /// <param name="dstY">Y-Location to draw</param>
        /// <param name="dstW">Width</param>
        /// <param name="dstH">Height</param>
		public void DrawSurface(Surface surface, int dstX, int dstY, int dstW, int dstH)
		{
			surface.CloneTo(this.Surface, dstX, dstY, dstW, dstH);
			//Modified = true;
		}
        /// <summary>
        /// Draws a Texture to the Canvas
        /// </summary>
        /// <param name="texture">The Texture to draw</param>
        /// <param name="dstX">X-Location to draw</param>
        /// <param name="dstY">Y-Location to draw</param>
        /// <param name="dstW">Width</param>
        /// <param name="dstH">Height</param>
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
        /// <summary>
        /// Draws a Material to the Canvas
        /// </summary>
        /// <param name="material">The Material to draw</param>
		public void DrawMaterial(Material material)
		{
			GL.Viewport(0, 0, Surface.Width, Surface.Height);
			
			Surface.BindFramebuffer();
			material.Use();
			Helper.DrawScreenQuad();
			GL.UseProgram(0);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
        /// <summary>
        /// Draws a text string to the Canvas
        /// </summary>
        /// <param name="text">The string to be drawn</param>
        /// <param name="shader">The shader to use for drawing</param>
        /// <param name="font">The font to use for drawing</param>
        /// <param name="charDistance">The distance between characters (can be negative)</param>
        /// <param name="position">The location where the string will be drawn</param>
        /// <param name="color">The color of the text</param>
		public void DrawText(string text, Shader shader, Font font, float charDistance, Vector2 position, Vector4 color)
		{
			font.DrawString(Surface, shader, text, charDistance, position, color);
			//Modified = true;
		}
        /// <summary>
        /// Draws a white text string to the Canvas
        /// </summary>
        /// <param name="text">The string to be drawn</param>
        /// <param name="shader">The shader to use for drawing</param>
        /// <param name="font">The font to use for drawing</param>
        /// <param name="charDistance">The distance between characters (can be negative)</param>
        /// <param name="position">The location where the string will be drawn</param>
		public void DrawText(string text, Shader shader, Font font, float charDistance, Vector2 position)
		{
			font.DrawString(Surface, shader, text, charDistance, position, Vector4.One);
			//Modified = true;
		}
	}
}
