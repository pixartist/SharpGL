using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Globalization;
namespace SharpGL.Drawing
{
	public class Mesh
	{
        /// <summary>
        /// An Attribute contains a vertex buffer and the information required to pass it to a shader.
        /// </summary>
        public class Attribute
        {
            private AttributeHint[] drawHints;
            private float[] data;
            public int VBO { get; private set; }
            public int Stride { get; private set; }
            public bool BufferUpdated { get; private set; }
            public int VertexArrayLength { get; private set; }
            /// <summary>
            /// The number of elements in the data array
            /// </summary>
            public int Length
            {
                get
                {
                    return data.Length;
                }
            }
            /// <summary>
            /// Indicates if this attribute has DrawHints which are required to pass it to the Shader
            /// </summary>
            public bool HasDrawHints
            {
                get
                {
                    return drawHints != null && drawHints.Length > 0;
                }
            }
            /// <summary>
            /// The number of elements described by this attribute
            /// </summary>
            public int ElementCount
            {
                get
                {
                    return VertexArrayLength / Stride;
                }
            }
            /// <summary>
            /// Creates a vertex buffer attribute
            /// </summary>
            /// <param name="data"></param>
            public Attribute(float[] data)
            {
                VBO = -1;
                
                BufferUpdated = false;
                this.data = data;
            }
            /// <summary>
            /// Changes the vertex data for this Attribute
            /// </summary>
            /// <param name="data">A list of floating point numbers</param>
            public void SetData(float[] data)
            {
                this.data = data;
                BufferUpdated = false;
            }
            /// <summary>
            /// Draw hints are used to describe the contents of this buffer when passing it to the shader
            /// </summary>
            /// <param name="hints">An arbitrary number of draw hints</param>
            public void SetDrawHints(params AttributeHint[] hints)
            {
                this.drawHints = hints;
                BufferUpdated = false;
            }
            /// <summary>
            /// Passes the data from this buffer to the attribute(s) in the shader
            /// </summary>
            /// <param name="shader"></param>
            public void Apply(Shader shader)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                shader.SetVertexAttributes(drawHints);
            }
            /// <summary>
            /// Updates the vertex buffer
            /// </summary>
            public void UpdateBuffer()
            {
                if (!BufferUpdated)
                {
                    BufferUpdated = true;
                    Stride = -1;
                    VertexArrayLength = -1;
                    foreach (var dh in drawHints)
                    {
                        if (dh.stride > Stride)
                            Stride = dh.stride;
                    }

                    if (data != null)
                    {
                        if (VBO < 0)
                        {
                            VBO = GL.GenBuffer();
                        }
                        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                        GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
                        VertexArrayLength = data.Length;
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    }
                    else if (VBO >= 0)
                    {
                        GL.DeleteBuffer(VBO);
                    }

                }
            }
            /// <summary>
            /// Deletes the buffer from the OpenGL context
            /// </summary>
            public void DeleteBuffer()
            {
                if (VBO > 0)
                {
                    GL.DeleteBuffer(VBO);
                }
            }
            /// <summary>
            /// Tries to assemble a list of 3D points from the vertex data
            /// </summary>
            /// <param name="hintName">The draw hint describing the list of vertices</param>
            /// <returns>A list of points as described by the given draw hint</returns>
            public List<Vector3> GetPoints(string hintName = "_pos")
            {
                List<Vector3> v = new List<Vector3>();
                var hints = from x in drawHints where x.attributeName == hintName select x;
                //no hints given, iterate vertices freely
                if (hints == null ? true : hints.Count() < 1)
                {

                    for (int i = 0; i < data.Length - 2; i++)
                    {
                        v.Add(new Vector3(data[i], data[i + 1], data[i + 2]));
                    }

                }
                else //use hints
                {
                    var hint = hints.First();
                    //must be a 3-component system
                    if (hint.components != 3)
                        throw (new InvalidOperationException("Vertices must have 3 components"));

                    for (int i = hint.offset; i < data.Length - 2; i += hint.stride)
                    {
                        v.Add(new Vector3(data[i], data[i + 1], data[i + 2]));
                    }
                }
                return v;
            }
            
        }
        public bool BufferUpdated { get; private set; }
        private uint[] indices;
        public int VEO { get; private set; }
        public int IndexArrayLength { get; private set; }
        public Dictionary<string, Attribute> Attributes { get; private set; }
        public int ElementCount
        {
            get
            {
                if (VEO > 0)
                {
                    return IndexArrayLength;
                }
                else if (Attributes.Count > 0)
                {
                    return Attributes.First().Value.ElementCount;
                }
                else
                    return 0;
            }
        }
		public Mesh()
		{
            Attributes = new Dictionary<string, Attribute>();
			VEO = -1;
            BufferUpdated = false;
		}

        public void SetIndices(uint[] indices)
        {
            this.indices = indices;
            BufferUpdated = false;
        }
        public void SetVertices(float[] data)
        {
            Attribute a;
            if(!Attributes.TryGetValue("Vertex", out a))
            {
                a = new Attribute(data);
                a.SetDrawHints(new AttributeHint("_pos", 3, 3, 0, false));
                Attributes.Add("Vertex", a);
            }
            else
            {
                a.SetData(data);
            }
        }
        public void SetTexureCoordinates(float[] data)
        {
            Attribute a;
            if (!Attributes.TryGetValue("TexCoord", out a))
            {
                a = new Attribute(data);
                a.SetDrawHints(new AttributeHint("_texCoord", 2, 2, 0, false));
                Attributes.Add("TexCoord", a);
            }
            else
            {
                a.SetData(data);
            }
        }
        public void SetNormals(float[] data)
        {
            Attribute a;
            if (!Attributes.TryGetValue("Normal", out a))
            {
                a = new Attribute(data);
                a.SetDrawHints(new AttributeHint("_normal", 3, 3, 0, false));
                Attributes.Add("Normal", a);
            }
            else
            {
                a.SetData(data);
            }
        }
		public void ApplyAttributes(Shader shader)
		{
            if (VEO > 0)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, VEO);
            foreach (var a in Attributes)
                a.Value.Apply(shader);
		} 
		public void UpdateBuffers()
		{
            foreach (var a in Attributes.Values)
                a.UpdateBuffer();
            if (!BufferUpdated)
            {
                BufferUpdated = true;
                IndexArrayLength = -1;

                if (indices != null)
                {
                    if (VEO < 0)
                    {
                        VEO = GL.GenBuffer();
                    }
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, VEO);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
                    IndexArrayLength = indices.Length;
                }
                else if (VEO >= 0)
                {
                    GL.DeleteBuffer(VEO);
                }
            }
		}
		
		public void DeleteBuffers()
		{
            foreach (var a in Attributes.Values)
                a.DeleteBuffer();
            if (VEO > 0)
            {
                GL.DeleteBuffer(VEO);
            }
		}
       
        public List<Vector3> GetPoints(string hintName = "_pos")
        {
            if (Attributes.Count > 0)
                return Attributes.First().Value.GetPoints(hintName);
            return null;
        }
        public List<Vector3> GetPoints(string attribute, string hintName = "_pos")
        {
            return Attributes[attribute].GetPoints(hintName);
        }
        private static float FConvert(string v)
        {
           // if (String.IsNullOrEmpty(v))
              //  return 0f;
            return float.Parse(v, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.GetCultureInfo("en-US").NumberFormat);
        }
        public static Mesh LoadOBJ(string filePath)
        {

                string[] objData = File.ReadAllLines(filePath);
                Mesh m = new Mesh();
                
                string[][] splitLines = new string[objData.Length][];
                for(int i = 0; i < objData.Length; i++)
                {
                    splitLines[i] = objData[i].Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                }
                float[][] vertexList = ((from l in splitLines where l.Length > 0 && l[0] == "v" select Array.ConvertAll<string, float>(l.Skip(1).ToArray<string>(), FConvert))).ToArray();

                if (vertexList == null || vertexList.Length < 1)
                {
                    throw (new Exception("No vertex data found in mesh file"));
                }
                float[][] texCoordList = ((from l in splitLines where l.Length > 0 && l[0] == "vt" select Array.ConvertAll<string, float>(l.Skip(1).ToArray<string>(), FConvert))).ToArray();
                float[][] normalList = ((from l in splitLines where l.Length > 0 && l[0] == "vn" select Array.ConvertAll<string, float>(l.Skip(1).ToArray<string>(), FConvert))).ToArray();
                var faces = (from l in splitLines where l.Length > 0 && l[0] == "f" && l.Length == 4 select l.Skip(1).ToArray()).ToArray<string[]>(); // only taking 3-component faces
                if(faces == null || faces.Length < 1)
                {
                    throw (new Exception("No face data found in mesh file"));
                }
                int fComps = faces[0].Length;
                int vComps = vertexList[0].Length;
                float[] vertices = vertexList.SelectMany(i => i).ToArray();
                float[] texCoords = null;
                int tcComps = 0;
                if(texCoordList != null && texCoordList.Length > 0)
                {
                    tcComps = texCoordList[0].Length;
                    texCoords = new float[faces.Length * fComps * tcComps];
                    
                }
                float[] normals = null;
                int nComps = 0;
                if (normalList != null && normalList.Length > 0)
                {
                    nComps = normalList[0].Length;
                    normals = new float[faces.Length * fComps * nComps];
                    
                }
                uint[] indices = new uint[faces.Length * fComps];
                uint p = 0;
                for(int i = 0; i < faces.Length; i++)
                {
                    for (int k = 0; k < faces[i].Length; k++)
                    {

                        int n = i + k;
                        string[] f = faces[i][k].Split('/');
                        indices[p] = uint.Parse(f[0]) - 1;
                        if (f.Length > 1 && texCoords != null && f[1] != null && f[1].Length > 0)
                        {
                            Array.Copy(texCoordList[uint.Parse(f[1]) - 1], 0, texCoords, (int)(indices[p] * tcComps), tcComps);
                        }
                        if (f.Length > 2 && normals != null && f[2] != null && f[2].Length > 0)
                        {
                            Array.Copy(normalList[uint.Parse(f[2]) - 1], 0, normals, (int)(indices[p] * nComps), nComps);
                        }
                        p++;
                    }
                    
                }
                

                m.SetVertices(vertices);
                m.SetIndices(indices);
                if (texCoords != null && texCoords.Length > 0)
                    m.SetTexureCoordinates(texCoords);
                if (normals != null && normals.Length > 0)
                    m.SetNormals(normals);
                return m;

        }
	}
}
