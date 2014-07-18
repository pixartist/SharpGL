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
namespace SharpGL.Factories
{
	public class PrimitiveFactory
	{
		public Mesh Cube { get; private set; }
		public Mesh Plane { get; private set; }
		public PrimitiveFactory()
		{
			Cube = CreateCube(Vector3.One);
			Plane = CreatePlane(0, 0, 0, 1, 1, 1, 1, Quaternion.Identity);
		}
		private Mesh CreateCube(Vector3 size)
		{
			Mesh mesh = new Mesh();
			float[] vertices = new float[] {
				        0,         0,         0, -1, -1, -1, //0
				 + size.X,         0,         0,  1, -1, -1, //1
				        0,  + size.Y,         0, -1,  1, -1, //2
				 + size.X,  + size.Y,         0,  1,  1, -1, //3
				        0,         0,  + size.Z, -1, -1,  1, //4
				 + size.X,         0,  + size.Z,  1, -1,  1, //5
				        0,  + size.Y,  + size.Z, -1,  1,  1, //6
				 + size.X,  + size.Y,  + size.Z,  1,  1,  1 //7
			};
			mesh.SetVertices(vertices);
			uint[] indices = new uint[] {
				0,2,1,1,2,3, //front
				6,4,7,7,4,5, //back
				2,6,3,3,6,7, //top
				4,0,5,5,0,1, //bottom
				0,4,2,2,4,6, //left
				5,1,7,7,1,3 //right
			};
			mesh.SetIndices(indices);
			//mesh.SetDrawHints(new VertexObjectDrawHint("pos", 3, 6, 0, false));
			mesh.SetDrawHints(new VertexObjectDrawHint("pos", 3, 6, 0, false), new VertexObjectDrawHint("normal", 3, 6, 3, true));
			return mesh;
		}
		
		private Mesh CreatePlane(float x, float y, float z, float width, float depth, int segmentsX, int segmentsZ, Quaternion rotation)
		{
			int verticeX = segmentsX + 1;
			int verticeZ = segmentsZ + 1;
			float sx = 1f/verticeX;
			float sz = 1f/verticeZ;
			float[] vertices = new float[verticeX * verticeZ * 6];
			uint index;
			Vector3 tmp;
			Mesh mesh = new Mesh();
			for (int ix = 0; ix < verticeX; ix++)
			{
				for(int iz = 0; iz < verticeZ; iz++)
				{
					index = (uint)(ix * verticeZ + iz) * 6;
					tmp = new Vector3(x + width * sx * ix * 2, 0, y + depth * sz * iz * 2);
					tmp = Vector3.Transform(tmp, rotation);
					tmp += Vector3.Transform(Vector3.UnitY, rotation) * z ;
					vertices[index] = tmp.X;
					vertices[index + 1] = tmp.Y;
					vertices[index + 2] = tmp.Z;
					vertices[index + 3] = 0;
					vertices[index + 4] = 1;
					vertices[index + 5] = 0;
				}
				
			}
			uint[] indices = new uint[(segmentsX) * (segmentsZ) * 6];
			uint vIndex;
			for (uint ix = 0; ix < segmentsX; ix++)
			{
				for (uint iz = 0; iz < segmentsZ; iz++)
				{
					index = (uint)(ix * segmentsZ + iz) * 6;
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
			mesh.SetDrawHints(new VertexObjectDrawHint("pos", 3, 6, 0, false), new VertexObjectDrawHint("normal", 3, 6, 3, true));
			return mesh;
		}
	}
}
