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
    /// <summary>
    /// A SurfaceFormat object contains all required information required to initialize a Texture object. This includes pixel data.
    /// </summary>
	public struct SurfaceFormat
	{
		public PixelInternalFormat InternalFormat ;
		public TextureWrapMode WrapMode ;
		public PixelFormat PixelFormat ;
		public PixelType SourceType ;
		public TextureTarget TextureTarget;
		public IntPtr Pixels ;
		public bool DepthBuffer;
		public bool MipMapping ;
		public int Multisampling ;
		public SurfaceFormat(
			PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8,
			TextureWrapMode wrapMode = TextureWrapMode.Repeat,
			PixelFormat pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
			PixelType sourceType = PixelType.UnsignedByte,
			TextureTarget TextureTarget = TextureTarget.Texture2D,
			IntPtr? pixels = null,
			int multisampling = 0,
			bool depthBuffer = true,
			bool Mimapping = false
		)
		{
			this.InternalFormat = internalFormat;
			this.WrapMode = wrapMode;
			this.PixelFormat = pixelFormat;
			this.SourceType = sourceType;
			this.Multisampling = multisampling;
			this.DepthBuffer = depthBuffer;
			this.TextureTarget = TextureTarget;
			this.MipMapping = Mimapping;
            if (pixels.HasValue)
                this.Pixels = pixels.Value;
            else
                this.Pixels = IntPtr.Zero;
		}
		public static SurfaceFormat Texture2D
		{
			get
			{
			return new SurfaceFormat( PixelInternalFormat.Rgb16, TextureWrapMode.Repeat, PixelFormat.Rgb, PixelType.Float, TextureTarget.Texture2D, null, 0, false, true);
			}
		}
		public static SurfaceFormat Texture2DAlpha
		{
			get
			{
				return new SurfaceFormat(PixelInternalFormat.Rgba16, TextureWrapMode.Repeat, PixelFormat.Rgba, PixelType.Float, TextureTarget.Texture2D, null, 0, false, true);
			}
		}
		public static SurfaceFormat Surface2D
		{
			get
			{
				return new SurfaceFormat(PixelInternalFormat.Rgb16, TextureWrapMode.Repeat, PixelFormat.Rgb, PixelType.Float, TextureTarget.Texture2D, null, 0, true, false);
			}
		}
		public static SurfaceFormat Surface2DAlpha
		{
			get
			{
				return new SurfaceFormat(PixelInternalFormat.Rgba16, TextureWrapMode.Repeat, PixelFormat.Rgba, PixelType.Float, TextureTarget.Texture2D, null, 0, true, false);
			}
		}

	}
    /// <summary>
    /// A Texture2D contains image data for rendering
    /// </summary>
	public class Texture2D : IDisposable
	{
		
		protected bool disposed = false;
        protected int textureHandle = 0;
        /// <summary>
        /// The SurfaceFormat used to initialize this texture object
        /// </summary>
        public SurfaceFormat Format { get; protected set; }
        /// <summary>
        /// The width in pixels of this texture
        /// </summary>
        public int Width { get; protected set; }
        /// <summary>
        /// The height in pixels of this texture
        /// </summary>
        public int Height { get; protected set; }
        /// <summary>
        /// The texture handle id
        /// </summary>
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
        /// <summary>
        /// Creates an empty Texture2D with the default Texture2D format
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
		public Texture2D(int width, int height)
		{
			Create(width, height, SurfaceFormat.Texture2D);
		}
        /// <summary>
        /// Creates a Texture2D
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="format">The texture format to be used</param>
		public Texture2D(int width, int height, SurfaceFormat format)
		{
			Create(width, height, format);
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
		public Texture2D(string filePath)
		{
			CreateFromPNG(filePath);
		}
        /// <summary>
        /// Creates a Texture2D
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="format">The texture format to be used</param>
		protected virtual void Create(int width, int height, SurfaceFormat format)
		{
			this.Format = format;
			Width = width;
			Height = height;
			textureHandle = GL.GenTexture();
			//bind texture
			
			GL.BindTexture(format.TextureTarget, textureHandle);
			Log.Error("Bound Texture: " + GL.GetError());
			if (format.TextureTarget == TextureTarget.Texture2D)
			{

				GL.TexParameter(format.TextureTarget, TextureParameterName.TextureMinFilter, (int)(format.MipMapping ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
				GL.TexParameter(format.TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(format.TextureTarget, TextureParameterName.TextureWrapS, (int)format.WrapMode);
				GL.TexParameter(format.TextureTarget, TextureParameterName.TextureWrapT, (int)format.WrapMode);
			}
            
			Log.Debug("Created Texture Parameters: " + GL.GetError());

			GL.TexImage2D(format.TextureTarget, 0, format.InternalFormat, Width, Height, 0, format.PixelFormat, format.SourceType, format.Pixels);
			if (format.MipMapping)
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			Log.Debug("Created Image: " + GL.GetError());
			//unbind texture
			GL.BindTexture(format.TextureTarget, 0);
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
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
        /// <summary>
        /// Binds the texture to the given texture slot (default: 0)
        /// </summary>
        /// <param name="slot">The OpenGL texture slot</param>
        public void BindTexture(TextureUnit slot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(slot);
            GL.BindTexture(Format.TextureTarget, textureHandle);
            
        }
        /// <summary>
        /// Sets the pixels of a given area of the given TextureTarget
        /// </summary>
        /// <typeparam name="T">The data type of the pixel data</typeparam>
        /// <param name="target">The OpenGL TextureTarget</param>
        /// <param name="format">The pixel format</param>
        /// <param name="type">The pixel type</param>
        /// <param name="x">X-coordinate of the rectangle to be written to</param>
        /// <param name="y">Y-coordinate of the rectangle to be written to</param>
        /// <param name="w">Width of the rectangle to be written to</param>
        /// <param name="h">Height of the rectangle to be written to</param>
        /// <param name="pixels">The pixel data</param>
        public static void SetPixels<T>(TextureTarget target, PixelFormat format, PixelType type, int x, int y, int w, int h, T pixels) where T : struct
        {
            GL.TexSubImage2D(target, 0, x, y, w, h, format, type, ref pixels);
        }
        /// <summary>
        /// Sets the pixels of the given area on this texture
        /// </summary>
        /// <typeparam name="T">The data type of the pixel data</typeparam>
        /// <param name="x">X-coordinate of the rectangle to be written to</param>
        /// <param name="y">Y-coordinate of the rectangle to be written to</param>
        /// <param name="w">Width of the rectangle to be written to</param>
        /// <param name="h">Height of the rectangle to be written to</param>
        /// <param name="pixels">The pixel data</param>
        public void SetPixels<T>(int x, int y, int w, int h, PixelType pixelType, T pixels) where T : struct
        {
            BindTexture();
            Texture2D.SetPixels(TextureTarget.Texture2D, Format.PixelFormat, pixelType, x, y, w, h, pixels);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
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
