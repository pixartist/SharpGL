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
		private int vao;
		private Mesh mesh;
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
			mesh = new Mesh();
			mesh.SetVertices(new float[] {
				-1,-1,
				1,-1,
				1,1,
				-1,1
			});
			mesh.SetIndices(new uint[] {
				2,3,0,
				0,1,2
			});
			mesh.SetDrawHints(new VertexObjectDrawHint("pos", 2, 2, 0, false));
			mesh.UpdateBuffers();
			vao = GL.GenVertexArray();
		}
		public void Clear(float r = 0, float g = 0, float b = 0, float a = 0)
		{
			Surface.Clear(r, g, b, a);
		}
		public void DrawSurface(Surface surface, int x, int y, int width, int height)
		{
			surface.CloneTo(this.Surface, x, -y, width, height);
			//Modified = true;
		}
		public void DrawTexture(Texture2D texture, int x, int y, int width, int height)
		{
			int fbo = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
			GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, fbo, 0);
			Surface.BindFramebuffer(FramebufferTarget.DrawFramebuffer);
			GL.BlitFramebuffer(0, 0, texture.Width, texture.Height, x, y, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
		}
		public void DrawMaterial(Material material, int x, int y, int width, int height)
		{
			GL.Viewport(-1, -1, 2, 2);
			GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VBO);
			GL.BindVertexArray(vao);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VEO);
			material.Use();
			mesh.ApplyDrawHints(material.Shader);

			GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.UseProgram(0);
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
