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
    /// <summary>
    /// Creates default GameObjects
    /// </summary>
	public class GameObjectFactory
	{
		public App App { get; private set; }
        /// <summary>
        /// Creates a GameObjectFactory
        /// </summary>
        /// <param name="app">The owning application</param>
		public GameObjectFactory(App app)
		{
			App = app;
		}
        /// <summary>
        /// Creates a GameObject with a cube mesh attached
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="scale">The Scale</param>
        /// <param name="size">Not used</param>
        /// <returns>A GameObject with a cube mesh in a MeshRenderer</returns>
		public GameObject CreateCube(Vector3 position, Vector3 scale, Vector3 size)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Cube;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["lit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
        /// <summary>
        /// Creates a GameObject with a cube mesh attached
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="scale">The Scale</param>
        /// <returns>A GameObject with a cube mesh in a MeshRenderer</returns>
		public GameObject CreateCube(Vector3 position, Vector3 scale)
		{
			GameObject result = App.CreateGameObject("Cube");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Cube;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["lit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
        /// <summary>
        /// Creates a GameObject with a plane mesh attached
        /// </summary>
        /// <param name="scale">The scale of the plane</param>
        /// <param name="position">The world location of the GameObject</param>
        /// <returns>A GameObject with a plane mesh in a MeshRenderer</returns>
		public GameObject CreatePlane(Vector3 scale, Vector3 position)
		{
			GameObject result = App.CreateGameObject("Plane");
			result.Transform.LocalPosition = position;
			result.Transform.LocalScale = scale;
			MeshRenderer r = result.AddComponent<MeshRenderer>();
			r.Mesh = App.PrimitiveFactory.Plane;
			r.PrimitiveType = PrimitiveType.Triangles;
			r.Material = App.Materials["lit"];
			r.Parameters.SetParameter<float>("_color", 1, 1, 1, 1);
			return result;
		}
	}
}
