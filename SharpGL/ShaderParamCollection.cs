using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Drawing;
namespace SharpGL
{
	public class ShaderParamCollection
	{
		public Dictionary<string, ShaderParamBase> Paramters { get; private set; }
		public ShaderParamCollection()
		{
			Paramters = new Dictionary<string, ShaderParamBase>();
		}
		public void SetParameter<T>(string name, params T[] values)
		{
			ShaderParamBase param;
			if (!Paramters.TryGetValue(name, out param))
			{
				Paramters.Add(name, new ShaderParam<T>(name, values));
			}
			else
			{
				Paramters[name].Values = values;
			}
		}
	}
}
