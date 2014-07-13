using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

using OpenTK.Graphics.OpenGL;
namespace SharpGL.Drawing
{
    public class Surface : IDisposable
    {
        public class SurfaceFormat
        {
            public PixelInternalFormat InternalFormat = PixelInternalFormat.Rgba8;
            public TextureWrapMode WrapMode = TextureWrapMode.Repeat;
            public PixelFormat PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
            public PixelType SourceType = PixelType.UnsignedByte;
            public IntPtr Pixels = IntPtr.Zero;
            public bool DepthBuffer = false;
            public SurfaceFormat(
                PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8,
                TextureWrapMode wrapMode = TextureWrapMode.Repeat,
                PixelFormat pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType sourceType = PixelType.UnsignedByte,
                bool depthBuffer = false,
                IntPtr? pixels = null
                )
            {
                this.InternalFormat = internalFormat;
                this.WrapMode = wrapMode;
                this.PixelFormat = pixelFormat;
                this.SourceType = sourceType;
                if (pixels.HasValue)
                    this.Pixels = pixels.Value;
                this.DepthBuffer = depthBuffer;
            }
        }
        private bool disposed = false;
        private int fboHandle = 0;
        private int textureHandle = 0;
        private int dbHandle = 0;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ID
        {
            get
            {
                return textureHandle;
            }
        }
        public void Create(int width, int height)
        {
            Create(width, height, new SurfaceFormat());
        }
        public void Create(int width, int height, SurfaceFormat format)
        {
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
            
            Log.Error("Created Texture Parameters: " + GL.GetError());
            GL.TexImage2D(TextureTarget.Texture2D, 0, format.InternalFormat, Width, Height, 0, format.PixelFormat, format.SourceType, format.Pixels);
            Log.Error("Created Image: " + GL.GetError());
            //unbind texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
            //create depthbuffer
            if (format.DepthBuffer)
            {
                GL.GenRenderbuffers(1, out dbHandle);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, dbHandle);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            }

            //create fbo
            fboHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboHandle);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, textureHandle, 0);
            if(format.DepthBuffer)
                GL.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, dbHandle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Log.Error("Created Framebuffer: " + GL.GetError());
            
        }
        public void CreateFromPNG(string filePath)
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
        public void CloneTo(Surface target)
        {
            GL.Viewport(0, 0, target.Width, target.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1.0, 1.0, 0.0, 0.0, 4.0);
            GL.UseProgram(0);
            target.Clear();
            
            BindTexture();
            target.BindFramebuffer();


            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(0, 0, 0);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(0, 1, 0);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(1, 1, 0);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(1, 0, 0);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void BindTexture(TextureUnit slot = TextureUnit.Texture0)
        {
            GL.ActiveTexture(slot);
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            
        }
        public void BindFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboHandle);
        }
        public void Clear(float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 0.0f)
        {
            BindFramebuffer();
            GL.ClearColor(r,g,b,a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
                GL.DeleteFramebuffer(fboHandle);
            }

            disposed = true;
        }
    }
}
