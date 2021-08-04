using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public static class Random
    {
        public static int Seed { get; private set; }

        private static System.Random random;

        static Random()
        {
            Seed = new System.Random().Next();
            random = new System.Random(Seed);
        }

        public static void InitState(int seed)
        {
            Seed = seed;
            random = new System.Random(Seed);
        }

        public static int Next() => random.Next();
        public static int Next(int min, int max) => random.Next(min, max);
        public static float Next(float min, float max) => min + NextFloat() / float.MaxValue * (max - min);

        public static float NextFloat()
        {
            var sign = random.Next(2);
            var exponent = random.Next((1 << 8) - 1);
            var mantissa = random.Next(1 << 23);

            var bits = (sign << 31) + (exponent << 23) + mantissa;
            return IntBitsToFloat(bits);
        }

        private static float IntBitsToFloat(int bits)
        {
            unsafe
            {
                return *(float*)&bits;
            }
        }
    }
}
