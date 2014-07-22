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
namespace SharpGL.Drawing
{
	public class Gui : MeshRenderer
{
		public float CameraDistance { get; set; }
		private Surface drawBuffer;
		internal override void Init()
		{
			base.Init();
		}
		public void Setup(int width, int height)
		{
			Camera cam;
			if (TryGetActiveCamera(out cam))
			{
				if (drawBuffer != null)
					drawBuffer.Dispose();
				Vector2 s = cam.NearplaneSize;
				Mesh = GameObject.App.PrimitiveFactory.Plane;//.CreatePlane(s.X / -2, s.Y / -2, -(cam.ZNear + 0.001f), s.X, s.Y, 1, 1, Quaternion.FromAxisAngle(Vector3.UnitX, Mathf.Deg2Rad(90)));
				drawBuffer = new Surface(width, height, new SurfaceFormat { DepthBuffer = false });
				Material = new Drawing.Material(App.Shaders["unlit"], true);
				
				Material.AddTexture("_tex", drawBuffer);
				PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
				drawBuffer.Clear(1,0,0,1);
			}
		}
		public void DrawSurface(Surface texture, int x, int y, int width, int height)
		{
			/*
			float _x = (float)x / drawBuffer.Width;
			float _y = (float)-y / drawBuffer.Height;
			float _w = (float)width / drawBuffer.Width;
			float _h = (float)height / drawBuffer.Height;
			 * */
			texture.CloneTo(drawBuffer, x, -y, width, height);
		}
	}
}
