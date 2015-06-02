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
    /// <summary>
    /// The SceneRenderer renders an entire scene to a given target
    /// </summary>
	public class SceneRenderer
	{
		private Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>> renderCalls;
		private int VAO;
		private App App;
        /// <summary>
        /// If true, the renderer will use multisamplers in order to render transparency.
        /// </summary>
		public bool UseAlphaToCoverage { get; set; }
        /// <summary>
        /// The color of the ambient light
        /// </summary>
        public Vector3 AmbientLight { get; set; }
        /// <summary>
        /// The color of the skylight
        /// </summary>
        public Vector3 SkylightColor { get; set; }
        /// <summary>
        /// The direction of the skylight
        /// </summary>
        public Vector3 SkylightDirection { get; set; }
        /// <summary>
        /// Creates a SceneRenderer
        /// </summary>
        /// <param name="app">The app this scenerenderer will run in</param>
		public SceneRenderer(App app)
		{
			this.App = app;
			renderCalls = new Dictionary<Mesh, Dictionary<Material, List<MeshRenderer>>>();
			VAO = GL.GenVertexArray();
			UseAlphaToCoverage = true;
		}
        /// <summary>
        /// Adds a renderer component to the SceneRenderer.
        /// </summary>
        /// <param name="renderer">The renderer to be added</param>
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
        /// <summary>
        /// Removes a renderer component from the SceneRenderer
        /// </summary>
        /// <param name="renderer">The renderer to be removed</param>
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
        /// <summary>
        /// Renders the scene to a surface
        /// </summary>
        /// <param name="camera">The Camera to use</param>
        /// <param name="surface">The Surface the scene will be rendered to</param>
        /// <param name="time">The current time</param>
		public void RenderToSurface(Camera camera, Surface surface, float time)
		{
			surface.BindFramebuffer();
			Render(camera, time);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
        /// <summary>
        /// Renders the the scene to a multisampled buffer. This is required for AlphaToCoverage.
        /// </summary>
        /// <param name="camera">The Camera to use</param>
        /// <param name="surface">The Surface the scene will be rendered to</param>
        /// <param name="time">The current time</param>
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
					//GL.BindVertexArray(VAO);

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
                            
							mesh.Key.ApplyAttributes(mat.Key.Shader);
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
        /// <summary>
        /// Renders the scene to the screen
        /// </summary>
        /// <param name="camera">The camera to use</param>
        /// <param name="time">The current time</param>
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
					
					//GL.BindVertexArray(VAO);
					

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
							mesh.Key.ApplyAttributes(mat.Key.Shader);
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
						//GL.BindVertexArray(VAO);
						foreach (var mat in mesh.Value)
						{
							mat.Key.Use();
							mat.Key.Shader.SetUniform<float>("_time", time);
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
							mesh.Key.ApplyAttributes(mat.Key.Shader);
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
						//GL.BindVertexArray(VAO);
						foreach (var mat in mesh.Value)
						{
							mat.Key.Use();
							mat.Key.Shader.SetUniform<float>("_time", time);
							mat.Key.Shader.SetUniform<int>("_samplerCount", mat.Key.Textures.Count);
                            mat.Key.Shader.SetUniform<float>("_ambient", AmbientLight.X, AmbientLight.Y, AmbientLight.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightColor", SkylightColor.X, SkylightColor.Y, SkylightColor.Z);
                            mat.Key.Shader.SetUniform<float>("_skylightDirection", SkylightDirection.X, SkylightDirection.Y, SkylightDirection.Z);
							mesh.Key.ApplyAttributes(mat.Key.Shader);
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
