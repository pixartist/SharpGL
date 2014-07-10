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
namespace SharpGL
{
	class PrimitiveFactory
	{
		public static Mesh CreateCube(Vector3 size)
		{
			Mesh m = new Mesh();
			float[] vertices = new float[] {
				        0,         0,         0, 1.0f,1.0f,1.0f, //0
				 + size.X,         0,         0, 0.0f,1.0f,1.0f, //1
				        0,  + size.Y,         0, 1.0f,0.0f,1.0f, //2
				 + size.X,  + size.Y,         0, 1.0f,1.0f,0.0f, //3
				        0,         0,  + size.Z, 0.0f,0.0f,1.0f, //4
				 + size.X,         0,  + size.Z, 1.0f,0.0f,0.0f, //5
				        0,  + size.Y,  + size.Z, 0.0f,1.0f,0.0f, //6
				 + size.X,  + size.Y,  + size.Z, 0.0f,0.0f,0.0f  //7
			};
			m.SetVertices(vertices);
			uint[] indices = new uint[] {
				0,2,1,1,2,3, //front
				6,4,7,7,4,5, //back
				2,6,3,3,6,7, //top
				4,0,5,5,0,1, //bottom
				0,4,2,2,4,6, //left
				5,1,7,7,1,3 //right
			};
			m.SetIndices(indices);
			return m;
		}
		public static Mesh CreatePlane(int verticeX, int verticeZ)
		{
			return CreatePlane(0, 0, 1, 1, verticeX, verticeZ, Quaternion.Identity);
		}
		public static Mesh CreatePlane(int verticeX, int verticeZ, Quaternion rotation)
		{
			return CreatePlane(0, 0, 1, 1, verticeX, verticeZ, rotation);
		}
		public static Mesh CreatePlane(float x, float y, int verticeX, int verticeZ)
		{
			return CreatePlane(x, y, 1, 1, verticeX, verticeZ, Quaternion.Identity);
		}
		public static Mesh CreatePlane(float x, float y, int verticeX, int verticeZ, Quaternion rotation)
		{
			return CreatePlane(x, y, 1, 1, verticeX, verticeZ, rotation);
		}
		
		public static Mesh CreatePlane(float x, float y, float width, float depth, int verticeX, int verticeZ, Quaternion rotation)
		{
			float sx = 1f/verticeX;
			float sz = 1f/verticeZ;
			float[] vertices = new float[verticeX * verticeZ * 3];
			uint index;
			Vector3 tmp;
			Mesh mesh = new Mesh();
			for (int ix = 0; ix < verticeX; ix++)
			{
				for(int iz = 0; iz < verticeZ; iz++)
				{
					index = (uint)(ix * verticeZ + iz) * 3;
					tmp = new Vector3(x + width * sx * ix, 0, y + depth * sz * iz);
					tmp = Vector3.Transform(tmp, rotation);
					vertices[index] = tmp.X;
					vertices[index + 1] = tmp.Y;
					vertices[index + 1] = tmp.Z;
				}
			}
			uint[] indices = new uint[(verticeX - 1) * (verticeZ - 1) * 6];
			uint vIndex;
			for (uint ix = 0; ix < verticeX - 1; ix++)
			{
				for (uint iz = 0; iz < verticeZ - 1; iz++)
				{
					index = (uint)(ix * (verticeZ - 1) + iz) * 6;
					vIndex = (uint)((ix * verticeZ + iz) * 3);
					indices[index] = vIndex;
					indices[index + 1] = vIndex + 1;
					indices[index + 2] = (uint)(vIndex + verticeZ);
					indices[index + 3] = (uint)(vIndex + verticeZ);
					indices[index + 4] = vIndex + 1;
					indices[index + 5] = (uint)(vIndex + verticeZ + 1);
				}
			}
			mesh.SetVertices(vertices);
			mesh.SetIndices(indices);
			return mesh;
		}
	}
}
