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
	public class MeshRenderCore
	{
		private Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>> renderCalls;
		private int VAO;
		private App App;
		public bool UseAlphaToCoverage { get; set; }
		public MeshRenderCore(App app)
		{
			this.App = app;
			renderCalls = new Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>>();
			VAO = GL.GenVertexArray();
			UseAlphaToCoverage = true;
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
					dict.Add(renderer.Material, renderers);
				}
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
			camera.App.ResetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthFunc(DepthFunction.Less);
			GL.DepthMask(true);
			bool doRender;
			Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>> transpCalls = null;
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
						doRender = true;
						if (!camera.Multisampling || !UseAlphaToCoverage)
						{
							//No multisampling -> Depth Peeling
							if (mat.Key.RenderMode == RenderMode.Translucent)
							{
								doRender = false;
								if (transpCalls == null)
									transpCalls = new Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>>();
								Dictionary<Material, List<MeshRenderer>> dict;
								if (!transpCalls.TryGetValue(mesh.Key, out dict))
								{
									dict = new Dictionary<Material, List<MeshRenderer>>();
									transpCalls.Add(mesh.Key, dict);
								}
								List<MeshRenderer> renderers;
								if (!dict.TryGetValue(mat.Key, out renderers))
								{
									renderers = new List<MeshRenderer>();
									dict.Add(mat.Key, renderers);
								}
								foreach (var c in mat.Value)
								{
									renderers.Add(c);
								}
							}
						}
						//Multisampling -> Alpha to Coverage
						if (doRender)
						{
							mat.Key.Use();
							mat.Key.Shader.SetUniform<float>("_time", time);
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
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
				}

				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}
			//Draw transparent meshes without color (if multisampling disabled)
			if (transpCalls != null)
			{
				GL.ColorMask(false, false, false, false);
				foreach (var mesh in transpCalls)
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
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								c.OnPreDraw();
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
								mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
								c.ApplyParameters(mat.Key.Shader);
								if (mesh.Key.VEO > 0)
									GL.DrawElements(c.PrimitiveType, elementCount, DrawElementsType.UnsignedInt, 0);
								else
									GL.DrawArrays(c.PrimitiveType, 0, elementCount);
								c.OnPostDraw();
							}
							GL.UseProgram(0);
						}
					}
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					GL.BindVertexArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}
				GL.ColorMask(true, true, true, true);
				//Draw again but with color
				GL.DepthFunc(DepthFunction.Lequal);
				GL.DepthMask(false);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha); 
				foreach (var mesh in transpCalls)
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
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								c.OnPreDraw();
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
								mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
								c.ApplyParameters(mat.Key.Shader);
								if (mesh.Key.VEO > 0)
									GL.DrawElements(c.PrimitiveType, elementCount, DrawElementsType.UnsignedInt, 0);
								else
									GL.DrawArrays(c.PrimitiveType, 0, elementCount);
								c.OnPostDraw();
							}
							GL.UseProgram(0);
						}
					}
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					GL.BindVertexArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}
				App.ResetBlendFunc();
				GL.DepthFunc(DepthFunction.Less);
				GL.DepthMask(true);
			}
		}
	}
}
