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
		public bool Modified { get; private set; }
		public bool BlendAdditive { get; set; }
		private Surface drawBuffer;
		protected override void OnInit()
		{
			base.OnInit();
			BlendAdditive = false;
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
                SurfaceFormat format = SurfaceFormat.Surface2DAlpha;
                format.DepthBuffer = false;
                format.MipMapping = true;
                drawBuffer = new Surface(width, height, format);
				Material = new Drawing.Material(App.Shaders["unlit"], RenderMode.Translucent);

				Material.AddTexture("_tex", drawBuffer);
				PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
				drawBuffer.Clear(1, 0, 0, 1);
			}
			Modified = true;
		}
		public override void OnPreDraw()
		{
			if (Modified)
			{
				//drawBuffer.BindTexture();
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				//GL.BindTexture(TextureTarget.Texture2D, 0);
				Modified = false;
			}
			if (BlendAdditive)
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
		}
		public override void OnPostDraw()
		{
			if (BlendAdditive)
				App.ResetBlendFunc();
		}
		public void DrawSurface(Surface surface, int x, int y, int width, int height)
		{
			surface.CloneTo(drawBuffer, x, -y, width, height);
			Modified = true;
		}
		public void DrawText(string text, Font font, float charDistance, Vector2 position, Vector4 color)
		{
			font.DrawString(drawBuffer, App.Shaders["text"], text, charDistance, position, color);
			Modified = true;
		}
		public void DrawText(string text, Font font, float charDistance, Vector2 position)
		{
			font.DrawString(drawBuffer, App.Shaders["text"], text, charDistance, position, Vector4.One);
			Modified = true;
		}
		public override void OnDestroy()
		{
			drawBuffer.Dispose();
			drawBuffer = null;
			base.OnDestroy();
		}
	}
}
