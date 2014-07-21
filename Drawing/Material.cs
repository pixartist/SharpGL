using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
    public class Material : IDisposable
    {
        public struct Texture
        {
            /// <summary>
            /// Name of the sampler in the shader
            /// </summary>
            public string Name;
            public Surface Surface;
            public Texture(string name, Surface surface)
            {
                Name = name;
                Surface = surface;
            }
            public Texture(Surface surface)
            {
                Name = null;
                Surface = surface;
            }
        }
        public Shader Shader {get; set;}
		public bool Transparency { get; set; }
        public Dictionary<string, Surface> Textures { get; private set; }
		public ShaderParamCollection Parameters { get; private set; }
        
		public Material(Shader shader, bool transparency = false)
		{
			this.Transparency = transparency;
			Shader = shader;
			Textures = new Dictionary<string, Surface>();
			Parameters = new ShaderParamCollection();
		}
		public void AddTexture(string name, Surface texture)
		{
			Textures.Add(name, texture);
		}
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
