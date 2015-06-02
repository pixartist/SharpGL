using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
	public enum RenderMode
	{
		Opaque,
		Translucent
	}
    /// <summary>
    /// A Material contains a shader, textures and shader parameters.
    /// </summary>
    public class Material : IDisposable
    {
		
        public struct Texture
        {
            /// <summary>
            /// Name of the sampler in the shader
            /// </summary>
            public string Name;
			public Texture2D Surface;
            public Texture(string name, Texture2D surface)
            {
                Name = name;
                Surface = surface;
            }
			public Texture(Texture2D surface)
            {
                Name = null;
                Surface = surface;
            }
        }
        /// <summary>
        /// This materials shader
        /// </summary>
        public Shader Shader {get; set;}
        /// <summary>
        /// The mode in which this material is rendered (Opaque or Translucent)
        /// </summary>
		public RenderMode RenderMode { get; set; }
        /// <summary>
        /// The textures used by this material.
        /// </summary>
        public Dictionary<string, Surface> Textures { get; private set; }
        /// <summary>
        /// The parameters applied when this material is being used.
        /// </summary>
		public ShaderParamCollection Parameters { get; private set; }
        /// <summary>
        /// Create a Material using the specified Shader and RenderMode
        /// </summary>
        /// <param name="shader">The Shader this Material will use</param>
        /// <param name="renderMode">Specifies whether this material should use alpha blending</param>
		public Material(Shader shader, RenderMode renderMode)
		{
			RenderMode = renderMode;
			Shader = shader;
			Textures = new Dictionary<string, Surface>();
			Parameters = new ShaderParamCollection();
		}
        /// <summary>
        /// Adds a texture to this material
        /// </summary>
        /// <param name="name">The texture can be referenced by this name</param>
        /// <param name="texture">The texture object</param>
		public void AddTexture(string name, Surface texture)
		{
			Textures.Add(name, texture);
		}
        /// <summary>
        /// Uses this Material. Binds the textures, uses the shader program and applies uniforms and parameters.
        /// </summary>
        public void Use()
        {
			GL.Enable(EnableCap.Texture2D);
            if(Shader != null)
                Shader.Use();
            int k = 0;
			foreach (var t in Textures)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + k);
				t.Value.BindTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + k);
				if (t.Key != null)
				{
					Shader.SetUniform<int>(t.Key, new int[] { k++ });
				}
			}
            foreach (var p in Parameters.Paramters.Values)
            {
                p.Apply(Shader);
            }
        }
        /// <summary>
        /// Disposes this material including the Shader and Textures.
        /// </summary>
        public void Dispose()
        {
            foreach (var t in Textures)
            {
                t.Value.Dispose();
            }
            Shader.Dispose();
        }
    }
}
