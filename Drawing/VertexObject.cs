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
	
	public class VertexObject
	{
		private int verticeLength;
		public int VBO { get; private set; }
		public int VAO { get; private set; }
		public PrimitiveType PrimitiveType { get; set; }
		public VertexObject()
		{
			VBO = GL.GenBuffer();
			VAO = GL.GenVertexArray();
			PrimitiveType = PrimitiveType.Points;
		}
		public void SetVertices(float[] vertices)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
			verticeLength = vertices.Length;
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}
		public void Draw(Shader shader, params VertexObjectDrawHint[] hints)
		{
			if(verticeLength > 0)
			{
				int verticeCount = -1;
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BindVertexArray(VAO);
				foreach (var h in hints)
				{
					if (h.attributeName != null)
					{
						int posAtt = GL.GetAttribLocation(shader.Program, h.attributeName);
						if (posAtt >= 0)
						{
							GL.EnableVertexAttribArray(posAtt);
							GL.VertexAttribPointer(posAtt, h.components, VertexAttribPointerType.Float, false, h.stride * sizeof(float), h.offset * sizeof(float));
							if (verticeCount < 0)
								verticeCount = verticeLength / h.stride;
						}
					}
				}
				if (verticeCount > 0)
				{
					GL.DrawArrays(PrimitiveType, 0, verticeCount);
					GL.BindVertexArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}
			}
		}
	}
}
