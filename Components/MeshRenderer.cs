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
		public virtual Mesh Mesh { get; set; }
		
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
							if (Mesh != null)
							{
								if (Mesh.HasDrawHints)
								{
									if (Mesh.VerticeComponentCount > 0)
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
		
		internal override void Init()
		{
			VAO = GL.GenVertexArray();
			Shader = GameObject.App.Shaders["default"];
			PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
		}
		internal override void Render(float time)
		{
			if(CanRender)
			{
				int elementCount = Mesh.ElementCount;
				if (elementCount > 0)
				{
					GL.BindBuffer(BufferTarget.ArrayBuffer, Mesh.VBO);
					GL.BindVertexArray(VAO);
					if (Mesh.VEO > 0)
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.VEO);

					if (Shader != null)
					{

						Shader.SetUniform("_time", time);
						Shader.SetUniform<Matrix4>("_modelViewProjection", GameObject.App.ActiveCamera.GetModelViewProjectionMatrix(Transform));
						Shader.Use();
						//default shader vars
						
						Mesh.ApplyDrawHints(Shader);
					}
					if (Mesh.VEO > 0)
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
