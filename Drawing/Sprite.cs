using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
    public class Sprite
    {
        private Vector3d rotationAxis;
        public delegate void RenderEvent(Sprite s);
        public event RenderEvent OnRender;
        public float z;
        public float scale;
        public Vector2 position;
        public Vector2 size;
        public float rotation;
        public bool enabled;
       
        public Material Material { get; set; }
        public Sprite(Material material, float x, float y, float width, float height, float z)
        {
            this.rotationAxis = new Vector3d(0, 0, -1);
            this.position = new Vector2(x, y);
            this.size = new Vector2(width, height);
            this.rotation = 0;
            this.z = z;
            this.Material = material;
            this.enabled = true;
            this.scale = 1.0f;
        }
        internal void Draw(float shiftX = 0, float shiftY = 0)
        {
            if (enabled)
            {
                if (OnRender != null)
                    OnRender(this);
                Material.Use();
                float halfX = (size.X / 2)*scale;
                float halfY = (size.Y / 2)*scale;
                Vector2d p = new Vector2d(position.X + shiftX, position.Y + shiftY);
                GL.PushMatrix();
                GL.Translate(p.X, p.Y, 0);
                GL.Rotate(Mathf.Rad2Deg(rotation), rotationAxis);
                GL.Translate(-p.X, -p.Y, 0);
                GL.Begin(PrimitiveType.Quads);

                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex3(-halfX + p.X, halfY + p.Y, z);
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex3(-halfX + p.X, -halfY + p.Y, z);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex3(halfX + p.X, -halfY + p.Y, z);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(halfX + p.X, halfY + p.Y, z);

                GL.End();

                GL.PopMatrix();
            }
        }
        public void Dispose()
        {
            Material.Dispose();
        }
    }
}
