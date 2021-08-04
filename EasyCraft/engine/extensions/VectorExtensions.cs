using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EasyCraft.engine.extensions
{
    public static class VectorExtensions
    {
        public static float ToV1(this Vector2 v)
        {
            return v.X;
        }

        public static float ToV1(this Vector3 v)
        {
            return v.X;
        }

        public static float ToV1(this Vector4 v)
        {
            return v.X;
        }

        public static Vector2 ToV2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 ToV2(this Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector3 ToV3(this Vector2 v, float f = 0f)
        {
            return new Vector3(v.X, v.Y, f);
        }

        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 ToV4(this Vector2 v, float f = 0f)
        {
            return new Vector4(v.X, v.Y, f, f);
        }

        public static Vector4 ToV4(this Vector3 v, float f = 0f)
        {
            return new Vector4(v.X, v.Y, v.Z, f);
        }
    }
}
