using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL.Components;
using SharpGL.Factories;
namespace SharpGL.Drawing
{
	public class Gui : MeshRenderer
{
		public float CameraDistance { get; set; }
		internal override void Init()
		{
			base.Init();
			SetupMesh();
		}
		private void SetupMesh()
		{
			Camera cam;
			if (TryGetActiveCamera(out cam))
			{
				Vector2 s = cam.NearplaneSize;
				Mesh = GameObject.App.PrimitiveFactory.Plane;//.CreatePlane(s.X / -2, s.Y / -2, -(cam.ZNear + 0.001f), s.X, s.Y, 1, 1, Quaternion.FromAxisAngle(Vector3.UnitX, Mathf.Deg2Rad(90)));
				Material = GameObject.App.Materials["unlit"];
				PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
			}
		}
	}
}
