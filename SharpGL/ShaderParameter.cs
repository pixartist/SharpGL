using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Drawing;
namespace SharpGL.Drawing
{
    public abstract class ShaderParamBase
    {
        public string Name { get; protected set; }
        public object Values { get; set; }
        protected Type type;
        public T[] GetValues<T>()
        {
            return (T[])Values;
        }
        public void Apply(Shader shader)
        {
            shader.SetUniform(Name, type, Values);
        }
    }
    public class ShaderParam<T> : ShaderParamBase
    {
        public ShaderParam(string name, params T[] values)
        {
            Name = name;
            Values = values;
            type = typeof(T);
        }
    }
}
