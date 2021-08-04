using System.Collections;
using System.Collections.Generic;
using EasyCraft.engine;
using SharpDX;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using WIN = System.Windows.Forms;
using System.Threading;

namespace EasyCraft
{
    public class World : Behavior
    {
        public static World Instance { get; private set; }

        public Player player;
        public Vector3 spawnPoint;

        public int ViewDistance { get; set; } = 4;
        public int Seed { get; private set; } = 3434690;
        public int LoadedChunkCount { get => chunks.Count; }
        public int ActiveChunkCount { get => activeChunks.Count; }
        public ChunkCoord PlayerChunkCoord { get => playerChunkCoord; }
        public int HighestNonAir { get; private set; } = 0;
        public bool UpdateViewDistance { get; set; } = true;
        public bool WorldLoaded { get; private set; } = false;

        public Material ChunkMaterial { get; private set; }

        public GenerationScreen generationScreen;

        private BlockType[] blockTypes = new BlockType[] {
            new BlockType("Air", false),
            new BlockType("Stone", true, new int[]{0,0,0,0,0,0 }),
            new BlockType("Dirt", true, new int[]{1,1,1,1,1,1 }),
            new BlockType("Grass", true, new int[]{2,2,7,1,2,2 }),
            new BlockType("Bedrock", true, new int[]{9,9,9,9,9,9 }),
        };

        List<Biome> biomes = new List<Biome>();
        Dictionary<ChunkCoord, Chunk> chunks = new Dictionary<ChunkCoord, Chunk>();
        List<ChunkCoord> activeChunks = new List<ChunkCoord>();
        ChunkCoord playerChunkCoord;
        ChunkCoord playerLastChunkCoord;

        FileSystemWatcher resourceWatcher;

        public override void Awake()
        {
            if (Instance == null) Instance = this;

            LoadData();

            resourceWatcher = new FileSystemWatcher("easycraft");
            resourceWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            resourceWatcher.Changed += (s, e) => ReloadResources(true);
            resourceWatcher.Created += (s, e) => ReloadResources(true);

            resourceWatcher.IncludeSubdirectories = true;
            resourceWatcher.EnableRaisingEvents = true;
        }

        public void ReloadResources(bool threadLock)
        {
            if (threadLock)
            {
                App.ThreadLock();
                App.WaitForThread();
            }
            log("Reloading resources");
            dispose(ChunkMaterial);
            LoadData();

            foreach(Chunk c in chunks.Values)
                c.ReloadRender();

            player.Reload();
            if(threadLock)
                App.ThreadUnlock();
        }

        private void LoadData()
        {
            ChunkMaterial = new Material();
            ChunkMaterial.shader = Shader.FromFile("easycraft/shaders/terrain/default.hlsl");
            ChunkMaterial.textures.Add(ShaderTexture2D.FromFileArray(new string[] {
                "easycraft/textures/blocks/stone.png",
                "easycraft/textures/blocks/dirt.png",
                "easycraft/textures/blocks/grass_block.png",
                "easycraft/textures/blocks/coal.png",
                "easycraft/textures/blocks/oak_planks.png",
                "easycraft/textures/blocks/oak_log.png",
                "easycraft/textures/blocks/oak_log_top.png",
                "easycraft/textures/blocks/grass_block_top.png",
                "easycraft/textures/blocks/cobblestone.png",
                "easycraft/textures/blocks/bedrock.png",
                "easycraft/textures/blocks/sand.png",
                "easycraft/textures/blocks/bricks.png"
            }));

            foreach (string biome in Directory.GetFiles("easycraft/biomes", "*.json"))
                biomes.Add(Biome.FromJson(File.ReadAllText(biome), "easycraft"));
        }

        public void ReloadChunks()
        {
            activeChunks.Clear();
            foreach (Chunk c in chunks.Values) c.Destroy();
            chunks.Clear();
            WorldLoaded = false;
            new Task(new System.Action(() =>
            {
                GenerateWorld();

                WorldLoaded = true;
                ReloadResources(true);
            })).Start();
        }

        public override void Start()
        {
#if DEBUG
            // Test math functions
            
            // Array functions
            for (int i = 0; i < 50000; i++)
            {
                Vector2 pos2 = Mathf.GetVector2FromIndex(i, 16);
                int newind2 = Mathf.GetIndexFromVector2(pos2, 16);
                Vector2 newpos2 = Mathf.GetVector2FromIndex(newind2, 16);
                if (newind2 != i)
                    log($"Error [2]: {newind2} != {i} | {pos2}");
                if(newpos2 != pos2)
                    log($"Error [2]: {newpos2} != {pos2} | {newind2}");

                Vector3 pos3 = Mathf.GetVector3FromIndex(i, 16, 256);
                int newind3 = Mathf.GetIndexFromVector3(pos3, 16, 256);
                Vector3 newpos3 = Mathf.GetVector3FromIndex(newind3, 16, 256);
                if (newind3 != i)
                    log($"Error [3]: {newind3} != {i} | {pos3}");
                if (newpos3 != pos3)
                    log($"Error [3]: {newpos3} != {pos3} | {newind3}");
            }
#endif
            Random.InitState(Seed);
            new Task(new System.Action(() =>
            {
                spawnPoint = new Vector3(Random.Next(-100000, 100000), 255, Random.Next(-100000, 100000));
                spawnPoint = new Vector3(Random.Next(0, 100000), 255, Random.Next(0, 100000));
                spawnPoint.X = spawnPoint.Z = 0;
                player.transform.position = spawnPoint;
                HighestNonAir = GetMaxHeight(player.transform.position);
                player.transform.position = new Vector3(player.transform.position.X, spawnPoint.Y = HighestNonAir + 2.8f, player.transform.position.Z);
                playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
                GenerateWorld();
                generationScreen.active = false;
                WorldLoaded = true;
            })).Start();
        }

        public override void Update()
        {
            RenderSettings.FogStart = (ViewDistance - 1) * StaticData.ChunkWidth;
            RenderSettings.FogEnd = ViewDistance * StaticData.ChunkWidth;

            if (!WorldLoaded) return;

            HighestNonAir = GetMaxHeight(player.transform.position);

            if (Input.GetKey(WIN.Keys.G))
                player.transform.position = new Vector3(player.transform.position.X, HighestNonAir + 2.8f, player.transform.position.Z);

            if (Input.GetKey(WIN.Keys.H))
                player.transform.position = spawnPoint;

            playerChunkCoord = GetChunkCoordFromVector3(player.transform.position);
            if (UpdateViewDistance && !playerChunkCoord.Equals(playerLastChunkCoord))
                CheckViewDistance();
        }

        private void GenerateWorld()
        {
            log("Generating world");
            float startTime = Time.time;
            ChunkCoord spawncoord = GetChunkCoordFromVector3(spawnPoint);

            List<ChunkCoord> chunkGenerators = new List<ChunkCoord>();
            chunkGenerators.Add(spawncoord);

            while (chunkGenerators.Count > 0)
            {
                ChunkCoord c = chunkGenerators[0]; chunkGenerators.RemoveAt(0);

                ChunkCoord[] steps = new ChunkCoord[]
                {
                    new ChunkCoord(c.x - 1, c.z),
                    new ChunkCoord(c.x, c.z - 1),
                    new ChunkCoord(c.x + 1, c.z),
                    new ChunkCoord(c.x, c.z + 1)
                };

                foreach(ChunkCoord step in steps)
                {
                    if (step.x >= spawncoord.x - ViewDistance && step.x < spawncoord.x + ViewDistance && step.z >= spawncoord.x - ViewDistance && step.z < spawncoord.z + ViewDistance && !chunkGenerators.Contains(step) && !chunks.ContainsKey(step))
                        chunkGenerators.Add(step);
                }

                CreateChunk(c.x, c.z);
            }

            float endTime = Time.time;
            log("Generated world in " + (endTime - startTime) + " seconds");
        }

        private void CheckViewDistance()
        {
            playerLastChunkCoord = playerChunkCoord;
            return;
            ChunkCoord coord = GetChunkCoordFromVector3(player.transform.position);

            List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
            activeChunks.Clear();

            List<ChunkCoord> chunkGenerators = new List<ChunkCoord>();
            chunkGenerators.Add(coord);

            while (chunkGenerators.Count > 0)
            {
                ChunkCoord c = chunkGenerators[0]; chunkGenerators.RemoveAt(0);

                ChunkCoord[] steps = new ChunkCoord[]
                {
                    new ChunkCoord(c.x - 1, c.z),
                    new ChunkCoord(c.x, c.z - 1),
                    new ChunkCoord(c.x + 1, c.z),
                    new ChunkCoord(c.x, c.z + 1)
                };

                foreach (ChunkCoord step in steps)
                {
                    if (step.x >= coord.x - ViewDistance && step.x < coord.x + ViewDistance && step.z >= coord.x - ViewDistance && step.z < coord.z + ViewDistance && !chunkGenerators.Contains(step))
                        chunkGenerators.Add(step);
                }

                if(!chunks.ContainsKey(c))
                    CreateChunk(c.x, c.z);
                else if (!chunks[c].active)
                {
                    chunks[c].active = true;
                    if(!activeChunks.Contains(c))
                        activeChunks.Add(c);
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(c))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }

            foreach (ChunkCoord c in previouslyActiveChunks)
            {
                chunks[c].active = false;
            }
            previouslyActiveChunks.Clear();
        }

        public ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.X / StaticData.ChunkWidth);
            int z = Mathf.FloorToInt(pos.Z / StaticData.ChunkWidth);
            return new ChunkCoord(x, z);
        }

        public BlockType GetBlockType(int id)
        {
            if (id < 0 || id > blockTypes.Length - 1)
                return BlockType.Error;
            return blockTypes[id];
        }

        private void CreateChunk(Vector2 pos)
        {
            ChunkCoord c = new ChunkCoord(Mathf.FloorToInt(pos.X), Mathf.FloorToInt(pos.Y));
            if (!chunks.ContainsKey(c))
                chunks.Add(c, new Chunk(c));
            else log("Tried creating chunk that already exists! [" + c + "]");
            if(!activeChunks.Contains(c))
                activeChunks.Add(c);
        }

        private void CreateChunk(int x, int z)
        {
            ChunkCoord c = new ChunkCoord(x, z);
            if (!chunks.ContainsKey(c))
                chunks.Add(c, new Chunk(c));
            else log("Tried creating chunk that already exists! [" + c + "]");
            if (!activeChunks.Contains(c))
                activeChunks.Add(c);
        }

        public bool CheckForVoxel(float x, float y, float z)
        {
            int xCheck = Mathf.FloorToInt(x);
            int yCheck = Mathf.FloorToInt(y);
            int zCheck = Mathf.FloorToInt(z);

            //log("XYZ: " + xCheck + "/" + yCheck + "/" + zCheck);

            int xChunk = xCheck / StaticData.ChunkWidth;
            int zChunk = zCheck / StaticData.ChunkWidth;

            xCheck -= (xChunk * StaticData.ChunkWidth);
            zCheck -= (zChunk * StaticData.ChunkWidth);

            ChunkCoord c = new ChunkCoord(xChunk, zChunk);
            if (chunks.ContainsKey(c))
                return blockTypes[chunks[c].GetVoxelR(xCheck, yCheck, zCheck)].solid;
            else
            {
                log("CheckForVoxel::NO CHUNK");
                return blockTypes[GetVoxel(new Vector3(xChunk + xCheck, yCheck, zChunk + zCheck))].solid;
            }
        }

        public int GetVoxel(Vector3 pos)
        {
            if(WorldLoaded)
                log("Get: " + pos);
            int yPos = Mathf.FloorToInt(pos.Y);

            /* IMMUTABLE PASS */

            if (pos.Y < 0 || pos.Y > StaticData.ChunkHeight)
                return 0;

            if (yPos == 0)
                return 4;

            /* BASIC TERRAIN PASS */

            int terrainHeight = Mathf.FloorToInt(biomes[0].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.X, pos.Z), 0, biomes[0].terrainScale, Seed)) + biomes[0].solidGroundHeight;
            int resId;

            if (yPos == terrainHeight)
                resId = 3;
            else if (yPos < terrainHeight && yPos > terrainHeight - 4)
                resId = 2;
            else if (yPos > terrainHeight)
                resId = 0;
            else
                resId = 1;

            /* SECOND PASS */

            foreach (Biome.Lode lode in biomes[0].lodes)
            {
                if (lode.replaces.Contains(resId) && yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold, Seed))
                        resId = lode.blockID;
            }

            return resId;
        }

        public int GetMaxHeight(Vector3 pos)
        {
            ChunkCoord c = GetChunkCoordFromVector3(pos);
            int y = StaticData.ChunkHeight - 1;
            while (y > 0 &&
                !GetBlockType(
                    chunks.ContainsKey(c) ? chunks[c].GetVoxel(new Vector3(pos.X, y, pos.Z)) :
                    GetVoxel(new Vector3(pos.X, y, pos.Z))
                    ).solid
                ) y--;
            return y;
        }

        public Biome GetCurrentBiome()
        {
            return biomes[0];
        }

        public override void OnDestroy()
        {
            dispose(ChunkMaterial);
            dispose(resourceWatcher);
        }
    }

    public struct ChunkCoord
    {
        public int x;
        public int z;

        public ChunkCoord(int _x, int _z)
        {
            x = _x;
            z = _z;
        }

        public bool Equals(ChunkCoord other)
        {
            return other.x == x && other.z == z;
        }

        public override string ToString()
        {
            return "{X: " + x + ", Z: " + z + "}";
        }

        public string ToString(string format)
        {
            return string.Format(format, x, z);
        }
    }

    [System.Serializable]
    public class BlockType
    {
        /// <summary>
        /// Error block
        /// </summary>
        public static BlockType Error { get => new BlockType("Error", true, new int[] { -1, -1, -1, -1, -1, -1 }); }

        /// <summary>
        /// The name of the block
        /// </summary>
        public string name = "";

        /// <summary>
        /// Is the block solid
        /// </summary>
        public bool solid = false;

        /// <summary>
        /// Texture ids of the faces (Back, Front, Top, Bottom, Left, Right)
        /// </summary>
        public int[] textures = new int[6];

        /// <summary>
        /// Creates a new block type
        /// </summary>
        public BlockType()
        {

        }

        /// <summary>
        /// Creates a new block type
        /// </summary>
        /// <param name="_name">The name of the block</param>
        /// <param name="_solid">Is the block solid</param>
        public BlockType(string _name, bool _solid)
        {
            name = _name;
            solid = _solid;
        }

        /// <summary>
        /// Creates a new block type
        /// </summary>
        /// <param name="_name">The name of the block</param>
        /// <param name="_solid">Is the block solid</param>
        /// <param name="_textures">The face textures (Back, Front, Top, Bottom, Left, Right)</param>
        public BlockType(string _name, bool _solid, int[] _textures)
        {
            name = _name;
            solid = _solid;
            textures = _textures;
        }

        /// <summary>
        /// Returns the texture id of the face
        /// <paramref name="faceIndex">The index of the face (Back, Front, Top, Bottom, Left, Right)</paramref>
        /// </summary>
        public int GetTextureID(int faceIndex)
        {
            if (faceIndex >= 0 && faceIndex < textures.Length)
                return textures[faceIndex];

            Debug.LogError($"Invalid face index '{faceIndex}'! Must be 0 >= {faceIndex} < {textures.Length}.");
            return 0;
        }
    }
}