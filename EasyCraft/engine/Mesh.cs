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
            public readonly float Id;

            public VertexData(Vector3 position, Color4 color, Vector2 uv, Vector3 normal, float id)
            {
                Position = new Vector4(position, 1);
                Color = color;
                UV = uv;
                Normal = normal;
                Id = id;
            }
        }

        public Vector3[] vertices = new Vector3[0];
        public int[] triangles = new int[0];
        public Color4[] colors = new Color4[0];
        public Vector3[] normals = new Vector3[0];
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

                Vector2 _uv = Vector2.Zero;
                if (i < uv.Length) _uv = uv[i];

                float _id = 0;
                if (i < id.Length) _id = id[i];

                data[i] = new VertexData(vertices[i], color, new Vector2(_uv.X, 1f - _uv.Y), normal, _id);
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
    }
}