using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;

namespace EasyCraft.engine
{
    public class Mesh
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexData
        {
            public readonly Vector4 Position;
            public readonly Color4 Color;
            public readonly Vector2 UV;
            public readonly Vector3 Normal;
            public readonly Vector3 Tangent;
            public readonly float Id;

            public VertexData(Vector3 position, Color4 color, Vector2 uv, Vector3 normal, Vector3 tanget, float id)
            {
                Position = new Vector4(position, 1);
                Color = color;
                UV = uv;
                Normal = normal;
                Tangent = tanget;
                Id = id;
            }
        }

        public Vector3[] vertices = new Vector3[0];
        public int[] triangles = new int[0];
        public Color4[] colors = new Color4[0];
        public Vector3[] normals = new Vector3[0];
        public Vector3[] tangents = new Vector3[0];
        public Vector2[] uv = new Vector2[0];
        public float[] id = new float[0];

        public VertexData[] GetVertexData()
        {
            VertexData[] data = new VertexData[vertices.Length];
            for (int i = 0; i < data.Length; i++)
            {
                Color4 color = Color.White;
                if (i < colors.Length) color = colors[i];

                Vector3 normal = Vector3.Zero;
                if (i < normals.Length) normal = normals[i];

                Vector3 tangent = Vector3.Zero;
                if (i < tangents.Length) tangent = tangents[i];

                Vector2 _uv = Vector2.Zero;
                if (i < uv.Length) _uv = uv[i];

                float _id = 0;
                if (i < id.Length) _id = id[i];

                data[i] = new VertexData(vertices[i], color, new Vector2(_uv.X, 1f - _uv.Y), normal, tangent, _id);
            }
            return data;
        }

        public ushort[] GetTrianglesUShort()
        {
            ushort[] t = new ushort[triangles.Length];
            for (int i = 0; i < t.Length; i++) t[i] = (ushort)triangles[i];
            return t;
        }

        public uint[] GetTrianglesUInt()
        {
            uint[] t = new uint[triangles.Length];
            for (int i = 0; i < t.Length; i++) t[i] = (uint)triangles[i];
            return t;
        }

        public void RecalculateNormals()
        {
            normals = new Vector3[vertices.Length];

            int j = 0;
            for(int i = 0; i < triangles.Length; i += 3)
            {
                // Iterate each face
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i+1]];
                Vector3 c = vertices[triangles[i+2]];
                a.Normalize();
                b.Normalize();
                c.Normalize();

                Vector3 edge1 = b - a;
                Vector3 edge2 = c - a;

                normals[j] = Vector3.Cross(edge1, edge2);
                j++;
            }
        }

        public void RecalculateTangents()
        {
            tangents = new Vector3[normals.Length];

            for(int i = 0; i < normals.Length; i++)
            {
                Vector3 t1 = Vector3.Cross(normals[i], Vector3.ForwardLH);
                Vector3 t2 = Vector3.Cross(normals[i], Vector3.Up);
                tangents[i] = t1.Length() > t2.Length() ? t1 : t2;
            }
        }
    }
}