using System;
using D2D1 = SharpDX.Direct2D1;
using EasyCraft.engine;
using DW = SharpDX.DirectWrite;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Newtonsoft.Json;

namespace EasyCraft
{
    [Serializable]
    public class Biome
    {
        [JsonIgnore]
        public string FullID { get => package + ":" + id; }

        [Serializable]
        public class Lode
        {
            public string nodeName;
            public int blockID;
            public int minHeight;
            public int maxHeight;
            public float scale;
            public float threshold;
            public float noiseOffset;
            public int[] replaces;
        }

        public string id;
        public string friendlyName;
        public int solidGroundHeight;
        public int terrainHeight;
        public float terrainScale;

        public Lode[] lodes;

        [NonSerialized, JsonIgnore]
        private string package="unknown";

        public static Biome FromJson(string jsonText, string package)
        {
            Biome biome = JsonConvert.DeserializeObject<Biome>(jsonText);
            biome.package = package;
            return biome;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
