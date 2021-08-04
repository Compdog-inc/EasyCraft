using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using EasyCraft.engine;
using SharpDX.WIC;
using System.Diagnostics;

namespace EasyCraft.engine
{
    public static class Global
    {
        public static RenderForm window;
#if DEBUG
        public static D3D11.DeviceDebug debug;
#endif
        public static D3D11.Device3 device;
        public static D2D1.Device device2D;
        public static D3D11.DeviceContext deviceContext;
        public static D2D1.DrawingStateBlock stateBlock;
        public static SwapChain swapChain;
        public static D2D1.DeviceContext deviceContext2D;
        public static Surface surface;
        public static D2D1.Bitmap targetBitmap2D;
        public static ImagingFactory imagingFactory;
        public static D2D1.Factory factory2D;
        public static SharpDX.DirectWrite.Factory dwFactory;
        public static D3D11.DepthStencilState depthStencilState;
        public static D3D11.RasterizerState cullBackState;
        public static D3D11.RasterizerState cullFrontState;
        public static D3D11.RasterizerState cullNoneState;
        public static Process CurrentProcess;
        public static Viewport viewport;
    }
}
