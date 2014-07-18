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
namespace SharpGL
{
	public class MeshRenderCore
	{
		private Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>> renderCalls;
		private int VAO;
		public MeshRenderCore()
		{
			renderCalls = new Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>>();
			VAO = GL.GenVertexArray();
			
		}
		public void AddRenderer(MeshRenderer renderer)
		{
			if (renderer != null ? renderer.Mesh != null && renderer.Material != null : false)
			{
				Dictionary<Material, List<MeshRenderer>> dict;
				if (!renderCalls.TryGetValue(renderer.Mesh, out dict))
				{
					dict = new Dictionary<Material, List<MeshRenderer>>();
					renderCalls.Add(renderer.Mesh, dict);
				}
				List<MeshRenderer> renderers;
				if (!dict.TryGetValue(renderer.Material, out renderers))
				{
					renderers = new List<MeshRenderer>();
					renderers.Add(renderer);
					dict.Add(renderer.Material, renderers);
				}
				if (!renderers.Contains(renderer))
					renderers.Add(renderer);
			}
		}
		public void RemoveRenderer(MeshRenderer renderer)
		{
			if (renderer != null ? renderer.Mesh != null && renderer.Material != null : false)
			{
				Dictionary<Material, List<MeshRenderer>> dict;
				if (renderCalls.TryGetValue(renderer.Mesh, out dict))
				{
					List<MeshRenderer> renderers;
					if (dict.TryGetValue(renderer.Material, out renderers))
					{
						renderers.Remove(renderer);
					}
				}
			}
		}
		public void Render(Camera camera, float time)
		{
			foreach (var mesh in renderCalls)
			{
				mesh.Key.UpdateBuffers();
				int elementCount = mesh.Key.ElementCount;
				if (elementCount > 0)
				{
					GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.Key.VBO);
					GL.BindVertexArray(VAO);
					if (mesh.Key.VEO > 0)
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.Key.VEO);

					foreach (var mat in mesh.Value)
					{
						mat.Key.Use();
						mat.Key.Shader.SetUniform<float>("_time", time);
						
						mesh.Key.ApplyDrawHints(mat.Key.Shader);
						foreach (var c in mat.Value)
						{
							mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
							c.ApplyParameters(mat.Key.Shader);
							if (mesh.Key.VEO > 0)
								GL.DrawElements(c.PrimitiveType, elementCount, DrawElementsType.UnsignedInt, 0);
							else
								GL.DrawArrays(c.PrimitiveType, 0, elementCount);

							
						}
						GL.UseProgram(0);
					}
				}
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}
		}
	}
}
