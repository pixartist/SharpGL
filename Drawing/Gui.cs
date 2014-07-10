using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL.Components;
namespace SharpGL.Drawing
{
	class Gui : Component
{
		private Mesh mesh;
		public float CameraDistance { get; set; }
		private VertexObjectDrawHint[] drawHints;
		public bool CanRender
		{
			get
			{
				if (GameObject != null)
				{
					if (GameObject.App != null)
					{
						if (GameObject.App.ActiveCamera != null && mesh != null && drawHints != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		internal Gui(GameObject parent)
			: base(parent)
		{
			drawHints = new VertexObjectDrawHint[1];
			drawHints[0] = new VertexObjectDrawHint("pos", 3, 3, 0);
		}
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
				mesh = PrimitiveFactory.CreatePlane(s.X / -2, s.Y / -2, s.X, s.Y, 1, 1, Quaternion.FromAxisAngle(Vector3.UnitX, Mathf.Deg2Rad(90)));
			}
		}
		internal override void Render(float dT)
		{
			base.Render(dT);
		}
	}
}
