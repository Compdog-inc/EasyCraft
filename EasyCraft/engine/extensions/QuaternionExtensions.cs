using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EasyCraft.engine.extensions
{
    public static class QuaternionExtensions
    {
        public static void ToEuler(this Quaternion q, out Vector3 v)
        {
            float t0 = 2f * (q.W * q.X + q.Y * q.Z);
            float t1 = 1f - 2f * (q.X * q.X + q.Y * q.Y);
            v.X = (float)Math.Round(MathUtil.RadiansToDegrees((float)Math.Atan2(t0, t1)) * 1000f) / 1000f;

            float t2 = 2f * (q.W * q.Y - q.Z * q.X);
            t2 = t2 > 1f ? 1f : t2 < -1f ? -1f : t2;
            v.Y = (float)Math.Round(MathUtil.RadiansToDegrees((float)Math.Asin(t2)) * 1000f) / 1000f;

            float t3 = 2f * (q.W * q.Z + q.X * q.Y);
            float t4 = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
            v.Z = (float)Math.Round(MathUtil.RadiansToDegrees((float)Math.Atan2(t3, t4)) * 1000f) / 1000f;
        }

        public static Vector3 ToEuler(this Quaternion q)
        {
            ToEuler(q, out Vector3 v);
            return v;
        }

        public static void FromEulerAngles(float eulerX, float eulerY, float eulerZ, out Quaternion q)
        {
            float x = MathUtil.DegreesToRadians(eulerZ); // yaw
            float y = MathUtil.DegreesToRadians(eulerY); // pitch
            float z = MathUtil.DegreesToRadians(eulerX); // roll

            q.X = (float)Math.Sin(z / 2f) * (float)Math.Cos(y / 2f) * (float)Math.Cos(z / 2f) - (float)Math.Cos(z / 2f) * (float)Math.Sin(y / 2f) * (float)Math.Sin(x / 2f);
            q.Y = (float)Math.Cos(z / 2f) * (float)Math.Sin(y / 2f) * (float)Math.Cos(x / 2f) + (float)Math.Sin(z / 2f) * (float)Math.Cos(y / 2f) * (float)Math.Sin(x / 2f);
            q.Z = (float)Math.Cos(z / 2f) * (float)Math.Cos(y / 2f) * (float)Math.Sin(x / 2f) - (float)Math.Sin(z / 2f) * (float)Math.Sin(y / 2f) * (float)Math.Cos(x / 2f);
            q.W = (float)Math.Cos(z / 2f) * (float)Math.Cos(y / 2f) * (float)Math.Cos(x / 2f) + (float)Math.Sin(z / 2f) * (float)Math.Sin(y / 2f) * (float)Math.Sin(x / 2f);
        }

        public static void FromEulerAngles(Vector3 euler, out Quaternion q)
        {
            FromEulerAngles(euler.X, euler.Y, euler.Z, out q);
        }

        public static Quaternion FromEulerAngles(float eulerX, float eulerY, float eulerZ)
        {
            FromEulerAngles(eulerX, eulerY, eulerZ, out Quaternion q);
            return q;
        }

        public static Quaternion FromEulerAngles(Vector3 euler)
        {
            FromEulerAngles(euler, out Quaternion q);
            return q;
        }
    }
}