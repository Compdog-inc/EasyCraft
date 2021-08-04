using System.Collections;
using System.Collections.Generic;
using EasyCraft.engine;
using SharpDX;

namespace EasyCraft
{
    public static class Noise
    {
        public static float Get2DPerlin(Vector2 position, float offset, float scale, int seed)
        {
            position.X += (offset + seed + 0.1f);
            position.Y += (offset + seed + 0.1f);
            return Mathf.PerlinNoise(position.X / StaticData.ChunkWidth * scale, position.Y / StaticData.ChunkWidth * scale);
        }

        public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold, int seed)
        {
            float x = (position.X + offset + seed + 0.1f) * scale;
            float y = (position.Y + offset + seed + 0.1f) * scale;
            float z = (position.Z + offset + seed + 0.1f) * scale;

            float AB = Mathf.PerlinNoise(x, y);
            float BC = Mathf.PerlinNoise(y, z);
            float AC = Mathf.PerlinNoise(x, z);
            float BA = Mathf.PerlinNoise(y, x);
            float CB = Mathf.PerlinNoise(z, y);
            float CA = Mathf.PerlinNoise(z, x);

            if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
                return true;
            else
                return false;
        }
    }
}
