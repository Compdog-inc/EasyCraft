using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;

namespace EasyCraft.engine
{
    public static class RenderSettings
    {
        public static float FogStart { get; set; } = -1;
        public static float FogEnd { get; set; } = -1;
    }
}
