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
    /// A Surface is based on the Texture2D class. It adds writing functionality by providing a framebuffer.
    /// </summary>
    public class Surface : Texture2D
    {
        private int fboHandle = -1;
        private int dbHandle = -1;
        /// <summary>
        /// Creates an empty Surface with the default Surface format
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
		public Surface(int width, int height)
		{
			Create(width, height, new SurfaceFormat());
		}
        /// <summary>
        /// Creates a Surface
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="format">The texture format to be used</param>
		public Surface(int width, int height, SurfaceFormat format)
		{
			Create(width, height, format);
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
		public Surface(string filePath)
		{
			CreateFromPNG(filePath);
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
        /// <param name="format">The surface format to be used</param>
		public Surface(string filePath, SurfaceFormat format)
		{
			CreateFromPNG(filePath, format);
		}
        /// <summary>
        /// Creates a Surface
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="format">The texture format to be used</param>
		protected override void Create(int width, int height, SurfaceFormat format)
		{
			this.Format = format;
			bool multisample = format.Multisampling > 0;
			
			int samples = Math.Max(0, Math.Min(format.Multisampling, 4));
			format.TextureTarget = multisample ? TextureTarget.Texture2DMultisample : format.TextureTarget;
			format.MipMapping = format.MipMapping && format.TextureTarget == TextureTarget.Texture2D;
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
			if (samples < 1)
				GL.TexImage2D(format.TextureTarget, 0, format.InternalFormat, Width, Height, 0, format.PixelFormat, format.SourceType, format.Pixels);
			else
				GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, format.InternalFormat, Width, Height, true);
			if (format.MipMapping)
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			
			Log.Debug("Created Image: " + GL.GetError());
			//unbind texture
			GL.BindTexture(format.TextureTarget, 0);
			//create depthbuffer
			if (format.DepthBuffer)
			{
				GL.GenRenderbuffers(1, out dbHandle);
				GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, dbHandle);
				
				if(multisample)
					GL.RenderbufferStorageMultisample(RenderbufferTarget.RenderbufferExt, samples, RenderbufferStorage.DepthComponent24, Width, Height);
				else
					GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent24, Width, Height);
			}

			//create fbo
			fboHandle = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
			GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, format.TextureTarget, textureHandle, 0);

			if (format.DepthBuffer)
				GL.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, dbHandle);
			Log.Debug("Framebuffer status: " + GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt));
			Log.Debug("Created Framebuffer: " + GL.GetError());
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
			
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
		protected override void CreateFromPNG(string filePath)
		{
			CreateFromPNG(filePath, SurfaceFormat.Texture2DAlpha);
		}
        /// <summary>
        /// Loads an image file and creates the texture from it.
        /// </summary>
        /// <param name="filePath">The path to the image file</param>
        /// <param name="format">The surface format to be used</param>
		protected void CreateFromPNG(string filePath, SurfaceFormat format)
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
        /// Clones the content of this Surface to the target Surface
        /// </summary>
        /// <param name="target">The target to be cloned to</param>
        public void CloneTo(Surface target)
        {

			BindFramebuffer(FramebufferTarget.ReadFramebuffer);
			target.BindFramebuffer(FramebufferTarget.DrawFramebuffer);
			GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, target.Width, target.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }
        /// <summary>
        /// Clones the content of this Surface to the given rectangle on the target Surface
        /// </summary>
        /// <param name="target">The target to be cloned to</param>
        /// <param name="x">X-Coordinate of the target rectangle</param>
        /// <param name="y">Y-Coordinate of the target rectangle</param>
        /// <param name="width">Width of the target rectangle</param>
        /// <param name="height">Height of the target rectangle</param>
		public void CloneTo(Surface target, int x, int y, int width, int height)
		{
			BindFramebuffer(FramebufferTarget.ReadFramebuffer);
			target.BindFramebuffer(FramebufferTarget.DrawFramebuffer);
			GL.BlitFramebuffer(0, 0, Width, Height, x, y, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
			
		}
        /// <summary>
        /// Binds this Surface to the framebuffer
        /// </summary>
        /// <param name="target">The target slot to bind to</param>
        public void BindFramebuffer(FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
			if (Format.Multisampling > 1)
				GL.Enable(EnableCap.Multisample);
            GL.BindFramebuffer(target, fboHandle);
        }
        /// <summary>
        /// Clears this Surface with the given color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        public void Clear(float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 0.0f)
        {
            BindFramebuffer();
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected new virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Log.Write("Disposing texture");
                GL.DeleteTexture(textureHandle);
                GL.DeleteFramebuffer(fboHandle);
            }

            disposed = true;
        }
    }
}
