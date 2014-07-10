using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL.Components;
using SharpGL.Drawing;
namespace SharpGL
{
	public class GameObjectFactory
	{
		public App App { get; private set; }
		public GameObjectFactory(App app)
		{
			App = app;
		}
		public GameObject CreateCube(Vector3 position, Vector3 scale, Vector3 size)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.Position = position;
			result.Transform.Scale = scale;
			MeshComponent mc = result.AddComponent<MeshComponent>();
			mc.Mesh = PrimitiveFactory.CreateCube(size);
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Shader = App.Shaders["default"];
			r.SetDrawHints(new VertexObjectDrawHint("pos",3,6,0), new VertexObjectDrawHint("color", 3, 6, 3));
			return result;
		}
		public GameObject CreateCube(Vector3 position, Vector3 scale)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.Position = position;
			result.Transform.Scale = scale;
			MeshComponent mc = result.AddComponent<MeshComponent>();
			mc.Mesh = PrimitiveFactory.CreateCube(new Vector3(1,1,1));
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Shader = App.Shaders["default"];
			r.SetDrawHints(new VertexObjectDrawHint("pos", 3, 6, 0), new VertexObjectDrawHint("color", 3, 6, 3));
			return result;
		}
	}
}
