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
		private float[] vertices;
		private uint[] indices;
		public int VertexArrayLength { get; private set; }
		public int IndexArrayLength { get; private set; }
		public int VBO { get; private set; }
		public int VEO { get; private set; }
		public int Stride { get; private set; }
		public bool BuffersUpdated { get; private set; }
		
		
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
					return IndexArrayLength;
				}
				else
				{
					return VertexArrayLength / Stride;
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
			BuffersUpdated = false;
		}
		public void ApplyDrawHints(Shader shader)
		{
			shader.SetVertexAttributes(drawHints);
		}
		public void SetVertices(float[] vertices)
		{
			this.vertices = vertices;
			BuffersUpdated = false;
		}
		public void SetIndices(uint[] indices)
		{
			this.indices = indices;
			BuffersUpdated = false;
		}
		public void UpdateBuffers()
		{
			if(!BuffersUpdated)
			{
				Stride = -1;
				VertexArrayLength = -1;
				IndexArrayLength = -1;
				foreach (var dh in drawHints)
				{
					if (dh.stride > Stride)
						Stride = dh.stride;
				}
				
				if (vertices != null)
				{
					if(VBO < 0)
					{
						VBO = GL.GenBuffer();
					}
					GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
					GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
					VertexArrayLength = vertices.Length;
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					if (indices != null)
					{
						if (VEO < 0)
						{
							VEO = GL.GenBuffer();
						}
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, VEO);
						GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
						IndexArrayLength = indices.Length;
					}
					else if (VEO >= 0)
					{
						GL.DeleteBuffer(VEO);
					}
				}
				else if(VBO >= 0)
				{
					GL.DeleteBuffer(VBO);
				}
				BuffersUpdated = true;
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
