using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Dictionary<string, ShaderParamBase> parameters;
        public Material(Shader shader, params Texture[] textures)
        {
            Shader = shader;
            Textures = new List<Texture>(textures);
            parameters = new Dictionary<string, ShaderParamBase>();
            
        }
        public Material(Shader shader, Surface texture)
        {
            Shader = shader;
            Textures = new List<Texture>();
            Textures.Add(new Texture(texture));
            parameters = new Dictionary<string, ShaderParamBase>();

        }
        public Material(Surface texture)
        {
            Shader = null;
            Textures = new List<Texture>();
            Textures.Add(new Texture(texture));
            parameters = new Dictionary<string, ShaderParamBase>();
        }
        public Material(params Texture[] textures)
        {
            Shader = null;
            Textures = new List<Texture>(textures);
            parameters = new Dictionary<string, ShaderParamBase>();
        }
        public void Use()
        {
            if(Shader != null)
                Shader.Use();
            int k = 0;
            foreach (var t in Textures)
            {
                t.Surface.BindTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + k);
                if(t.Name != null)
                {
                    Shader.SetUniform<int>(t.Name, new int[] { k++ });
                }
            }
            foreach (var p in parameters.Values)
            {
                p.Apply(Shader);
            }
        }
        public void SetParameter<T>(string name, params T[] values)
        {
            ShaderParamBase param;
            if(!parameters.TryGetValue(name, out param))
            {
                parameters.Add(name, new ShaderParam<T>(name, values));
            }
            else
            {
                parameters[name].Values = values;
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
