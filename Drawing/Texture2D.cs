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
	public class SurfaceFormat
	{
		public PixelInternalFormat InternalFormat = PixelInternalFormat.Rgba8;
		public TextureWrapMode WrapMode = TextureWrapMode.Repeat;
		public PixelFormat PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
		public PixelType SourceType = PixelType.UnsignedByte;
		public IntPtr Pixels = IntPtr.Zero;
		public bool DepthBuffer = true;
		public int Multisampling = 0;
		public SurfaceFormat(
			PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8,
			TextureWrapMode wrapMode = TextureWrapMode.Repeat,
			PixelFormat pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
			PixelType sourceType = PixelType.UnsignedByte,
			IntPtr? pixels = null,
			int multisampling = 0,
			bool depthBuffer = true
		)
		{
			this.InternalFormat = internalFormat;
			this.WrapMode = wrapMode;
			this.PixelFormat = pixelFormat;
			this.SourceType = sourceType;
			this.Multisampling = multisampling;
			this.DepthBuffer = depthBuffer;
			if (pixels.HasValue)
				this.Pixels = pixels.Value;
		}
	}
	public class Texture2D : IDisposable
	{
		
		protected bool disposed = false;
        protected int textureHandle = 0;
		protected SurfaceFormat format;
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int ID
        {
            get
            {
                return textureHandle;
            }
        }
		internal Texture2D()
		{
		}
		public Texture2D(int width, int height)
		{
			Create(width, height, new SurfaceFormat());
		}
		public Texture2D(int width, int height, SurfaceFormat format)
		{
			Create(width, height, format);
		}
		public Texture2D(string filePath)
		{
			CreateFromPNG(filePath);
		}
		protected virtual void Create(int width, int height, SurfaceFormat format)
		{
			this.format = format;
			Width = width;
			Height = height;
			textureHandle = GL.GenTexture();
			//bind texture
			
			GL.BindTexture(TextureTarget.Texture2D, textureHandle);
			Log.Error("Bound Texture: " + GL.GetError());
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)format.WrapMode);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)format.WrapMode);
			
            
			Log.Debug("Created Texture Parameters: " + GL.GetError());

			GL.TexImage2D(TextureTarget.Texture2D, 0, format.InternalFormat, Width, Height, 0, format.PixelFormat, format.SourceType, format.Pixels);
			
			Log.Debug("Created Image: " + GL.GetError());
			//unbind texture
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}
        protected virtual void CreateFromPNG(string filePath)
        {
            //check if the file exists
            if (System.IO.File.Exists(filePath))
            {
                //make a bitmap out of the file on the disk
                System.Drawing.Bitmap textureBitmap = new System.Drawing.Bitmap(filePath);
                //get the data out of the bitmap
                System.Drawing.Imaging.BitmapData textureData =
                textureBitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        textureBitmap.PixelFormat
                    );

				SurfaceFormat format = new SurfaceFormat();
                format.Pixels = textureData.Scan0;
                format.SourceType = PixelType.UnsignedByte;
                if (textureBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    format.PixelFormat = PixelFormat.Bgra;

                }
                else if (textureBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    format.PixelFormat = PixelFormat.Bgr;
                }
                else
                {
                    Log.Error("PNG Pixel format not supported (" + filePath + ") -> " + textureBitmap.PixelFormat.ToString());
                    return;
                }

                Create(textureBitmap.Width, textureBitmap.Height, format);
                //free the bitmap data (we dont need it anymore because it has been passed to the OpenGL driver
                textureBitmap.UnlockBits(textureData);

            }
            else
                Log.Error("Image " + filePath + " not found");
        }
        public void BindTexture(TextureUnit slot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(slot);
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Log.Write("Disposing texture");
                GL.DeleteTexture(textureHandle);
            }

            disposed = true;
        }
	}
}
