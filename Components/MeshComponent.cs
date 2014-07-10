using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL.Drawing;
namespace SharpGL.Components
{
	public class MeshComponent : Component
	{
		public Mesh Mesh { get; set; }
		public MeshComponent(GameObject parent) : base	(parent)
		{

		}
	}
}
