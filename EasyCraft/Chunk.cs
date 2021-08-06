using System.Collections;
using System.Collections.Generic;
using EasyCraft.engine;
using SharpDX;
using System.Threading.Tasks;

namespace EasyCraft
{
    public class Chunk
    {
        public ChunkCoord coord;

        MeshRenderer chunkRenderer;
        int vertexIndex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();
        List<Color4> colors = new List<Color4>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<float> ids = new List<float>();

        int[] voxelMap;
        int mapWidth;
        int mapHeight;

        public bool active
        {
            get => chunkRenderer.active;
            set => chunkRenderer.active = value;
        }

        public Vector3 position
        {
            get { return chunkRenderer.transform.position; }
        }

        public void Destroy()
        {
            chunkRenderer.Destroy();
        }

        public Chunk(ChunkCoord _coord)
        {
            mapWidth = StaticData.ChunkWidth;
            mapHeight = StaticData.ChunkHeight;
            voxelMap = new int[mapWidth * mapHeight * mapWidth];

            coord = _coord;
            chunkRenderer = new MeshRenderer() { name = "Chunk_" + coord.ToString("{0}_{1}") };
            chunkRenderer.triangleSize = TriangleSize.UInt;
            chunkRenderer.transform.position = new Vector3(coord.x * mapWidth, 0f, coord.z * mapWidth);
            chunkRenderer.material = new Material(World.Instance.ChunkMaterial);

            PopulateVoxelMap();
            CreateMeshData();
            CreateMesh();
        }

        bool IsVoxelInChunk(int x, int y, int z)
        {
            return !(x < 0 || x > mapWidth - 1 || y < 0 || y > mapHeight - 1 || z < 0 || z > mapWidth - 1);
        }

        void PopulateVoxelMap()
        {
            for (int i = 0; i < voxelMap.Length; i++)
            {
                voxelMap[i] = World.Instance.GetVoxel(Mathf.GetVector3FromIndex(i, mapWidth, mapHeight) + position);
            }
        }

        void CreateMeshData()
        {
            ClearMeshData();
            for (int i = 0; i < voxelMap.Length; i++)
            {
                if (World.Instance.GetBlockType(voxelMap[i]).solid)
                    AddVoxelDataToChunk(Mathf.GetVector3FromIndex(i, mapWidth, mapHeight));
            }
        }

        public int GetVoxel(int index)
        {
            return voxelMap[index];
        }

        public int GetVoxelR(Vector3 pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.Z < 0)
            {
                return 0;
            }

            int index = Mathf.GetIndexFromVector3(pos, mapWidth, mapHeight);
            if (index < voxelMap.Length)
                return voxelMap[index];
            return 0;
        }

        public int GetVoxelR(float x, float y, float z)
        {
            return GetVoxelR(new Vector3(x, y, z));
        }

        public int GetVoxel(Vector3 pos)
        {
            pos -= position;
            int index = Mathf.GetIndexFromVector3(pos, mapWidth, mapHeight);
            if (index < voxelMap.Length && index >= 0)
                return voxelMap[index];
            return 0;
        }

        public int GetVoxel(float x, float y, float z)
        {
            return GetVoxel(new Vector3(x, y, z));
        }

        bool CheckVoxel(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.X);
            int y = Mathf.FloorToInt(pos.Y);
            int z = Mathf.FloorToInt(pos.Z);

            if (World.Instance.WorldLoaded && (x < 0 || z < 0))
            {
            }

            if (!IsVoxelInChunk(x, y, z))
                return World.Instance.GetBlockType(World.Instance.GetVoxel(pos + position)).solid;

            return World.Instance.GetBlockType(voxelMap[Mathf.GetIndexFromVector3(pos, mapWidth, mapHeight)]).solid;
        }

        void AddVoxelDataToChunk(Vector3 pos)
        {
            Color4 aointensity = new Color4(0.3f)
            {
                Alpha = 1f
            };

            for (int p = 0; p < 6; p++)
            {
                if (!CheckVoxel(pos + StaticData.faceChecks[p]))
                {
                    int id = voxelMap[Mathf.GetIndexFromVector3(pos, mapWidth, mapHeight)];

                    vertices.Add(pos + StaticData.voxelVerts[StaticData.voxelTris[p, 0]]);
                    vertices.Add(pos + StaticData.voxelVerts[StaticData.voxelTris[p, 1]]);
                    vertices.Add(pos + StaticData.voxelVerts[StaticData.voxelTris[p, 2]]);
                    vertices.Add(pos + StaticData.voxelVerts[StaticData.voxelTris[p, 3]]);

                    normals.Add(StaticData.voxelNormals[p]);
                    normals.Add(StaticData.voxelNormals[p]);
                    normals.Add(StaticData.voxelNormals[p]);
                    normals.Add(StaticData.voxelNormals[p]);

                    tangents.Add(StaticData.voxelTangents[p]);
                    tangents.Add(StaticData.voxelTangents[p]);
                    tangents.Add(StaticData.voxelTangents[p]);
                    tangents.Add(StaticData.voxelTangents[p]);

                    AddAO(pos, p, aointensity);
                    uvs.AddRange(StaticData.voxelUvs);
                    AddTexture(World.Instance.GetBlockType(id).GetTextureID(p));

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                    vertexIndex += 4;
                }
            }
        }

        void AddAO(Vector3 pos, int p, Color4 aointensity)
        {
            Color4[] aocolors = new Color4[4] { Color.White, Color.White, Color.White, Color.White };
            if (StaticData.faceChecks[p].Y > 0)
            {
                for (int ac = 0; ac < StaticData.aoChecks.Length; ac++)
                {
                    if (CheckVoxel(pos + StaticData.aoChecks[ac] + Vector3.Up))
                    {
                        if (StaticData.aoIndices[p, 0] > -1)
                            aocolors[StaticData.aoIndices[ac, 0]] -= aointensity;
                        if (StaticData.aoIndices[ac, 1] > -1)
                            aocolors[StaticData.aoIndices[ac, 1]] -= aointensity;
                    }
                }
            }
            else if (StaticData.faceChecks[p].Y == 0)
            {
                if (CheckVoxel(pos + StaticData.faceChecks[p] + Vector3.Down))
                {
                    aocolors[0] -= aointensity * 2;
                    aocolors[2] -= aointensity * 2;
                }
            }
            colors.AddRange(aocolors);
        }

        void AddTexture(int id)
        {
            ids.Add(id);
            ids.Add(id);
            ids.Add(id);
            ids.Add(id);
        }

        void ClearMeshData()
        {
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            triangles.Clear();
            colors.Clear();
            uvs.Clear();
            ids.Clear();
            vertexIndex = 0;
        }

        void CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.id = ids.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            chunkRenderer.mesh = mesh;
        }

        public void ReloadRender()
        {
            chunkRenderer.material.Dispose();
            chunkRenderer.material = new Material(World.Instance.ChunkMaterial);
        }
    }
}
