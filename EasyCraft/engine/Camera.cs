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
    public enum CameraRenderMode
    {
        PerFrame,
        ValueUpdate,
        Manual
    }

    public class Camera : Behavior
    {
        public static Camera main { get; set; }
        public static Camera current { get; private set; }

        public D3D11.DeviceContext renderContext = null;
        public CameraRenderMode renderMode = CameraRenderMode.PerFrame;

        public Color4 ClearColor = new Color4(0.73f, 0.82f, 0.99f, 1f);

        public D3D11.Texture2D renderTexture { get; private set; }
        public D3D11.Texture2D depthTexture { get; private set; }
        public D3D11.RenderTargetView renderTargetView;
        public D3D11.DepthStencilView depthStencilView;

        public Matrix ProjectionMatrix
        {
            get
            {
                return Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(fov), (float)Global.window.ClientSize.Width / (float)Global.window.ClientSize.Height, clippingNear, clippingFar);
            }
        }

        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.Invert(Matrix.RotationQuaternion(transform.rotation) * Matrix.Translation(transform.position));
            }
        }

        public float fov = 60f;
        public float clippingNear = 0.01f;
        public float clippingFar = 1000.0f;

        public Camera() : base()
        {
            AllowRender = false;
            SubscribeRender();
            if (main == null) main = this;
        }

        public void CreateBuffers(D3D11.Texture2D renderTexture, int MSAA)
        {
            this.renderTexture = renderTexture;
            renderTargetView = new D3D11.RenderTargetView(Global.device, renderTexture);

            D3D11.Texture2DDescription depthBufferDesc = new D3D11.Texture2DDescription()
            {
                Format = Format.D32_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = Global.window.ClientSize.Width,
                Height = Global.window.ClientSize.Height,
                SampleDescription = new SampleDescription(MSAA, 0),
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.DepthStencil,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            depthTexture = new D3D11.Texture2D(Global.device, depthBufferDesc);
            depthStencilView = new D3D11.DepthStencilView(Global.device, depthTexture);
        }

        public void ForceDispose()
        {
            dispose(renderTargetView);
            dispose(depthStencilView);
            dispose(renderTexture);
            dispose(depthTexture);
        }

        public override void Render(D3D11.DeviceContext context)
        {
            if (renderMode == CameraRenderMode.PerFrame)
                RenderTarget();
        }

        public override void OnDestroy()
        {
            ForceDispose();
        }

        private void Foreach<T>(List<T> list, Action<T> action)
        {
            try
            {
                List<T> tmp = new List<T>(list);
                tmp.ForEach(action);
                tmp.Clear();
                tmp = null;
            }
            catch { }
        }

        public void RenderTarget()
        {
            current = this;
            D3D11.DeviceContext context = renderContext;
            if (context == null) context = Global.deviceContext;

            // Init output
            context.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);

            // Clear buffers
            context.ClearRenderTargetView(renderTargetView, ClearColor);
            context.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1f, 0);

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.AllowRender && beh.active) beh.Render(context); });

            if (current == main)
            {
                Global.deviceContext2D.SaveDrawingState(Global.stateBlock);
                Global.deviceContext2D.BeginDraw();
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.AllowRender && beh.active) beh.Render2D(Global.deviceContext2D); });
                Global.deviceContext2D.EndDraw();
                Global.deviceContext2D.RestoreDrawingState(Global.stateBlock);
            }

            current = null;
        }
    }
}
