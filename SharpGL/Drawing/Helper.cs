using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
	public static class Helper
	{
		public static void DrawScreenQuad()
		{
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(0, 1);
			GL.Vertex2(-1, -1);

			GL.TexCoord2(1, 1);
			GL.Vertex2(1, -1);

			GL.TexCoord2(1, 0);
			GL.Vertex2(1, 1);

			GL.TexCoord2(0, 0);
			GL.Vertex2(-1, 1);

			
			GL.End();
		}
	}
}
