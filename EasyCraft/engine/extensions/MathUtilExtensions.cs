using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EasyCraft.engine.extensions
{
    public static class MathUtilExtensions
    {
        public static float CopySign(float x, float m)
        {
            if (m >= 0) return Math.Abs(x);
            else return -Math.Abs(x);
        }
    }
}
