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
	public class Mesh
	{
		private VertexObjectDrawHint[] drawHints;
		public int VerticeComponentCount { get; private set; }
		public int IndexCount { get; private set; }
		public int VBO { get; private set; }
		public int VEO { get; private set; }
		public int Stride { get; private set; }
		public bool HasDrawHints
		{
			get
			{
				if (drawHints != null)
					if (drawHints.Length > 0)
						return true;
				return false;
			}
		}
		public int ElementCount
		{
			get
			{
				if (VEO > 0)
				{
					return IndexCount;
				}
				else
				{
					return VerticeComponentCount / Stride;
				}
			}
		}
		public Mesh()
		{
			VBO = -1;
			VEO = -1;
		}
		public void SetDrawHints(params VertexObjectDrawHint[] drawHints)
		{

			this.drawHints = drawHints;
			Stride = 0;
			foreach (var dh in drawHints)
			{
				if (dh.stride > Stride)
					Stride = dh.stride;
			}
		}
		public void ApplyDrawHints(Shader shader)
		{
			shader.SetVertexAttributes(drawHints);
		}
		public void SetVertices(float[] vertices)
		{
			VerticeComponentCount = 0;
			if (vertices != null)
			{
				if(VBO <= 0)
				{
					VBO = GL.GenBuffer();
				}
				VBO = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
				VerticeComponentCount = vertices.Length;
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}
			else if(VBO > 0)
			{
				GL.DeleteBuffer(VBO);
			}
		}
		public void SetIndices(uint[] indices)
		{
			IndexCount = -1;
			if (indices != null)
			{
				if (VEO <= 0)
				{
					VEO = GL.GenBuffer();
				}
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, VEO);
				GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
				IndexCount = indices.Length;
			}
			else if (VEO > 0)
			{
				GL.DeleteBuffer(VEO);
			}
		}
		public void DeleteBuffers()
		{
			if (VBO > 0)
			{
				GL.DeleteBuffer(VBO);
			}
			if (VEO > 0)
			{
				GL.DeleteBuffer(VEO);
			}
		}
	}
}
