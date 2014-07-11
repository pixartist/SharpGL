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
			r.Mesh = PrimitiveFactory.CreateCube(size);
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Shader = App.Shaders["default"];
			return result;
		}
		public GameObject CreateCube(Vector3 position, Vector3 scale)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = PrimitiveFactory.CreateCube(new Vector3(1,1,1));
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Shader = App.Shaders["default"];
			return result;
		}
		public GameObject CreatePlane(Vector3 scale, Vector3 position)
		{
			GameObject result = App.CreateGameObject("Plane");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = PrimitiveFactory.CreatePlane(0, 0, 0, 1, 1, 1, 1, Quaternion.Identity);
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Shader = App.Shaders["default"];
			return result;
		}
	}
}
