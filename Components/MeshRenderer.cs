using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL.Drawing;
namespace SharpGL.Components
{
	public class MeshRenderer : Component
	{
		private VertexObjectDrawHint[] drawHints;
		public PrimitiveType PrimitiveType { get; set; }
		public int Components { get; private set; }
		public Shader Shader { get; set; }
		public int VAO { get; private set; }
		public int Stride { get; private set; }
		public bool CanRender
		{
			get
			{
				if (GameObject != null)
				{
					if (GameObject.App != null)
					{
						if (GameObject.App.ActiveCamera != null)
						{
							MeshComponent m = GameObject.Component<MeshComponent>();
							if (m != null && drawHints != null)
							{
								if (m.Mesh != null)
								{
									if (m.Mesh.VerticeComponentCount > 0)
									{
										return true;
									}
								}
							}
						}
					}
				}
				return false;
			}
		}
		public MeshRenderer(GameObject parent) : base	(parent)
		{

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
		internal override void Init()
		{
			VAO = GL.GenVertexArray();
		}
		internal override void Render(float time)
		{
			if(CanRender)
			{
				MeshComponent m = GameObject.Component<MeshComponent>();
				int elementCount = m.Mesh.GetElementCount(Stride);
				if (elementCount > 0)
				{
					GL.BindBuffer(BufferTarget.ArrayBuffer, m.Mesh.VBO);
					GL.BindVertexArray(VAO);
					if (m.Mesh.VEO > 0)
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, m.Mesh.VEO);

					if (Shader != null)
					{
						Shader.Use();
						//default shader vars
						Shader.SetUniform<float>("_time", new float[] { time });
						Shader.SetUniform<Matrix4>("_modelViewProjection", GameObject.App.ActiveCamera.GetModelViewProjectionMatrix(m.Transform));

						Shader.SetVertexAttributes(drawHints);

					}
					if (m.Mesh.VEO > 0)
						GL.DrawElements(PrimitiveType, elementCount, DrawElementsType.UnsignedInt, 0);
					else
						GL.DrawArrays(PrimitiveType, 0, elementCount);
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					GL.BindVertexArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					GL.UseProgram(0);
				}
			}
		}
		
		public override void Destroy()
		{
			GL.DeleteVertexArray(VAO);
			base.Destroy();
		}
	}
}
