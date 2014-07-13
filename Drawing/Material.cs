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
        public List<Texture> Textures { get; private set; }
		public ShaderParamCollection Parameters { get; private set; }
        public Material(Shader shader, params Texture[] textures)
        {
            Shader = shader;
            Textures = new List<Texture>(textures);
			Parameters = new ShaderParamCollection();
            
        }
        public Material(Shader shader, Surface texture)
        {
            Shader = shader;
            Textures = new List<Texture>();
            Textures.Add(new Texture(texture));
			Parameters = new ShaderParamCollection();
        }
        public Material(Surface texture)
        {
            Shader = null;
            Textures = new List<Texture>();
            Textures.Add(new Texture(texture));
			Parameters = new ShaderParamCollection();
        }
        public Material(params Texture[] textures)
        {
            Shader = null;
            Textures = new List<Texture>(textures);
			Parameters = new ShaderParamCollection();
        }
		public Material(Shader shader)
		{
			Shader = shader;
			Textures = new List<Texture>();
			Parameters = new ShaderParamCollection();
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
                t.Surface.BindTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + k);
                if(t.Name != null)
                {
                    Shader.SetUniform<int>(t.Name, new int[] { k++ });
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
                t.Surface.Dispose();
            }
            Shader.Dispose();
        }
    }
}
