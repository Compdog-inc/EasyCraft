using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Collections.Generic;

namespace EasyCraft.engine
{
    public class DirectionalLight : Light
    {
        public static DirectionalLight Current { get; private set; }

        public DirectionalLight() { Current = this; }
    }
}
