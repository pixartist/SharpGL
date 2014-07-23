using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using SharpGL.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace SharpGL
{
	public class Font
	{
		private const int maxWidth = 2048;
		private Dictionary<char, Vector2> charSizes;
		private Dictionary<char, Vector2> charLocations;
		private Texture2D texture;
		public Font(string font, string characters, float size)
		{
			//measure space
			charSizes = new Dictionary<char, Vector2>();
			charLocations = new Dictionary<char, Vector2>();
			System.Drawing.Font f = new System.Drawing.Font(font, size);
			using (var gTemp = Graphics.FromHwnd(IntPtr.Zero))
			{
				
				char[] chars = characters.ToCharArray();
				foreach (var c in chars)
				{
					if (!charSizes.ContainsKey(c)) 
					{
						var s = gTemp.MeasureString(c + "", f);
						charSizes.Add(c, new Vector2(s.Width, s.Height));
					}
				}
			}
			float width = 0;
			float lineWidth = 0;
			float height = 0;
			float lineHeight = 0;
			foreach(var entry in charSizes)
			{
				if (lineWidth + entry.Value.X < maxWidth)
					lineWidth += entry.Value.X;
				else
				{
					width = maxWidth;
					height += lineHeight;
					lineHeight = 0;
					lineWidth = 0;
				}
				if (entry.Value.Y > lineHeight)
					lineHeight = entry.Value.Y;
			}
			height += lineHeight;
			width = Math.Max(lineWidth, width);
			
			Bitmap fontTarget = new Bitmap((int)(0.5f + width), (int)(0.5f + height));
			using (Graphics gr = Graphics.FromImage(fontTarget))
			{
				gr.TextRenderingHint = TextRenderingHint.AntiAlias;	
				float x = 0;
				float y = 0;
				float columnHeight = -1;
				gr.Clear(Color.Transparent);
				foreach (var entry in charSizes)
				{
					if (x + entry.Value.X >= maxWidth)
					{
						x = 0;
						y += columnHeight;
						columnHeight = 0;
					}
					gr.DrawString(entry.Key + "", f, new SolidBrush(Color.White), new PointF(x, y));
					charLocations.Add(entry.Key, new Vector2(x, y));
					x += entry.Value.X;
					if (entry.Value.Y > columnHeight)
						columnHeight = entry.Value.Y;
				}
			}
		//	fontTarget.Save("test.png",	ImageFormat.Png);
			BitmapData data = fontTarget.LockBits(new Rectangle(0, 0, fontTarget.Width, fontTarget.Height), ImageLockMode.ReadOnly, fontTarget.PixelFormat);
			texture = new Texture2D(fontTarget.Width, fontTarget.Height,
				new SurfaceFormat { Pixels = data.Scan0, DepthBuffer = false, Multisampling = 0, PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra, SourceType = PixelType.UnsignedByte}
				);
			fontTarget.UnlockBits(data);
		}
		public void DrawString(ref Surface surface, Shader shader, string text, float charDist, Vector2 basePos, Vector4 color)
		{
			float xAt = -1;
			float yAt = -1;
			Vector2 pos;
			Vector2 size;
			Vector2 sizeQ;
			char[] chars = text.ToCharArray();
			GL.Enable(EnableCap.Texture2D);
			shader.Use();
			shader.SetUniform<float>("_color", color.X, color.Y, color.Z, color.W);
			surface.BindFramebuffer();
			texture.BindTexture();
			basePos = new Vector2(basePos.X / (float)surface.Width, basePos.Y / (float)surface.Height);
			GL.Begin(PrimitiveType.Quads);
			foreach(var c in chars)
			{
				if (charLocations.ContainsKey(c))
				{
					Vector2 scale = new Vector2(surface.Width / (float)texture.Width, surface.Height / (float)texture.Height);
					pos = new Vector2(charLocations[c].X / texture.Width, charLocations[c].Y / texture.Height);
					size = new Vector2(charSizes[c].X / texture.Width, charSizes[c].Y / texture.Height);
					sizeQ = new Vector2(size.X / scale.X, size.Y / scale.Y) ;
					GL.TexCoord2(pos.X, pos.Y);
					GL.Vertex2(basePos.X + xAt, basePos.Y + yAt);
					
					GL.TexCoord2(pos.X + size.X, pos.Y);
					GL.Vertex2(basePos.X + xAt + sizeQ.X, basePos.Y + yAt);

					GL.TexCoord2(pos.X + size.X, pos.Y + size.Y);
					GL.Vertex2(basePos.X + xAt + sizeQ.X, basePos.Y + yAt + sizeQ.Y);

					GL.TexCoord2(pos.X, pos.Y + size.Y);
					GL.Vertex2(basePos.X + xAt, basePos.Y + yAt + sizeQ.Y);


					xAt += sizeQ.X * charDist;
					//y += sizeQ.Y;
				}
			}
			GL.End();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.UseProgram(0);
		}
	}
}
