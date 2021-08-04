using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public static class Time
    {
        public static float time = 0f;
        public static float deltaTime = 0f;
        public static float fixedDeltaTime = 0.02f;
        public static float timeSinceLastFixedDelta = 0f;
    }
}
