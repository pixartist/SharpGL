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
	public class MeshRenderer : Component
	{
		private Mesh _mesh;
		private Material _material;
		public Mesh Mesh
		{
			get
			{
				return _mesh;
			}
			set
			{
				App.SceneRenderer.RemoveRenderer(this);
				_mesh = value;
				App.SceneRenderer.AddRenderer(this);
			}
		}
		public Material Material
		{
			get
			{
				return _material;
			}
			set
			{
				App.SceneRenderer.RemoveRenderer(this);
				_material = value;
				App.SceneRenderer.AddRenderer(this);
			}
		}
		public PrimitiveType PrimitiveType { get; set; }
		public int Components { get; private set; }
		public ShaderParamCollection Parameters { get; private set; }
		
		public int Stride { get; private set; }
		public bool CanRender
		{
			get
			{
				if(App != null)
				{
					if (App.ActiveCamera != null)
					{
						if (Mesh != null)
						{
							if (Mesh.HasDrawHints)
							{
								if (Mesh.VertexArrayLength > 0)
								{
									return true;
								}
							}
						}

					}
				}
				return false;
			}
		}
		
		protected override void OnInit()
		{
			Material = App.Materials["unlit"];
			PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
			Parameters = new ShaderParamCollection();
			App.SceneRenderer.AddRenderer(this);
		}
		public void ApplyParameters(Shader shader)
		{
			foreach (var p in Parameters.Paramters.Values)
				p.Apply(shader);
		}
		public void SetMesh(Mesh m)
		{
			App.SceneRenderer.RemoveRenderer(this);
			Mesh = m;
			App.SceneRenderer.AddRenderer(this);
		}
		public virtual void OnPreDraw()
		{
		}

		public virtual void OnPostDraw()
		{ 
		}
		public override void OnDestroy()
		{
			App.SceneRenderer.RemoveRenderer(this);
		}
	}
}
