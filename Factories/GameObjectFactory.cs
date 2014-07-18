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
namespace SharpGL.Factories
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
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Cube;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["unlit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
		public GameObject CreateCube(Vector3 position, Vector3 scale)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Cube;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["unlit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
		public GameObject CreatePlane(Vector3 scale, Vector3 position)
		{
			GameObject result = App.CreateGameObject("Plane");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Plane;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["unlit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
	}
}
