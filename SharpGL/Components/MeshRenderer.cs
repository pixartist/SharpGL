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
    /// <summary>
    /// A MeshRenderer is a component handling a Mesh as well as a Material. It registers the Mesh in the rendering system which handles drawing
    /// </summary>
	public class MeshRenderer : Component
	{
		private Mesh _mesh;
		private Material _material;
        /// <summary>
        /// The Mesh used by this renderer
        /// </summary>
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
        /// <summary>
        /// The Material used to render the Mesh
        /// </summary>
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
        /// <summary>
        /// The type of primitive defined by the Mesh vertices
        /// </summary>
		public PrimitiveType PrimitiveType { get; set; }
        
		/// <summary>
		/// MeshRenderer-Specific Shader Parameters
		/// </summary>
		public ShaderParamCollection Parameters { get; private set; }
		
		/// <summary>
		/// Returns true if the state of the MeshRender allows rendering
		/// </summary>
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
        /// <summary>
        /// Applies the MeshRenderes custom shader parameters to the given shader
        /// </summary>
        /// <param name="shader">The shader to apply the parameters to</param>
		public void ApplyParameters(Shader shader)
		{
			foreach (var p in Parameters.Paramters.Values)
				p.Apply(shader);
		}
        /// <summary>
        /// Sets the Mesh
        /// </summary>
        /// <param name="m">A Mesh Object</param>
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
			Mesh = null;
			App.SceneRenderer.RemoveRenderer(this);
		}
	}
}
