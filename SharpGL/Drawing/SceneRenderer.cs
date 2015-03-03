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
	public class SceneRenderer
	{
		private Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>> renderCalls;
		private int VAO;
		private App App;
		public bool UseAlphaToCoverage { get; set; }
        public Vector3 AmbientLight { get; set; }
        public Vector3 SkylightColor { get; set; }
        public Vector3 SkylightDirection { get; set; }
		public SceneRenderer(App app)
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

		public void RenderToSurface(Camera camera, Surface surface, float time)
		{
			surface.BindFramebuffer();
			Render(camera, time);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
		public void RenderMultisampled(Camera camera, Surface msBuffer, float time)
		{
			msBuffer.Clear();
			GL.Enable(EnableCap.Multisample);
			GL.Enable(EnableCap.SampleAlphaToCoverage);
			camera.App.ResetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthFunc(DepthFunction.Less);
			GL.DepthMask(true);

			msBuffer.BindFramebuffer();

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
						if (mat.Value.Count > 0)
						{
							mat.Key.Use();
							mat.Key.Shader.SetUniform<float>("_time", time);
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
                            mat.Key.Shader.SetUniform<float>("_ambient", AmbientLight.X, AmbientLight.Y, AmbientLight.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightColor", SkylightColor.X, SkylightColor.Y, SkylightColor.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightDirection", SkylightDirection.X, SkylightDirection.Y, SkylightDirection.Z);
                            
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
								mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
                                mat.Key.Shader.SetUniform<Matrix4>("_rotationMatrix", c.Transform.Rotation.ToMatrix4());
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
			}
			GL.Disable(EnableCap.Multisample);
			GL.Disable(EnableCap.SampleAlphaToCoverage);
		}
		public void Render(Camera camera, float time)
		{
			camera.App.ResetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthFunc(DepthFunction.Less);
			GL.DepthMask(true);
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
						//No multisampling -> Depth Peeling
						if (mat.Key.RenderMode == RenderMode.Translucent)
						{
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
						else
						{
							mat.Key.Use();
							mat.Key.Shader.SetUniform<float>("_time", time);
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
                            mat.Key.Shader.SetUniform<float>("_ambient", AmbientLight.X , AmbientLight.Y , AmbientLight.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightColor", SkylightColor.X, SkylightColor.Y, SkylightColor.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightDirection", SkylightDirection.X, SkylightDirection.Y, SkylightDirection.Z);
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
                                mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
                                mat.Key.Shader.SetUniform<Matrix4>("_rotationMatrix", c.Transform.Rotation.ToMatrix4());
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
			//Draw transparent meshes without color (if multisampling disabled) for depth masking
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
                                mat.Key.Shader.SetUniform<Matrix4>("_rotationMatrix", c.Transform.Rotation.ToMatrix4());
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
				//Draw again but with color and Lequal depth func
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
                            mat.Key.Shader.SetUniform<float>("_ambient", AmbientLight.X, AmbientLight.Y, AmbientLight.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightColor", SkylightColor.X, SkylightColor.Y, SkylightColor.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightDirection", SkylightDirection.X, SkylightDirection.Y, SkylightDirection.Z);
							mesh.Key.ApplyDrawHints(mat.Key.Shader);
							foreach (var c in mat.Value)
							{
								c.OnPreDraw();
								//Log.Debug("Rendering " + c.GameObject.Name); <- LAG LAG LAG
                                mat.Key.Shader.SetUniform<Matrix4>("_modelViewProjection", camera.GetModelViewProjectionMatrix(c.Transform));
                                mat.Key.Shader.SetUniform<Matrix4>("_rotationMatrix", c.Transform.Rotation.ToMatrix4());
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
