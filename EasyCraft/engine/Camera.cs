using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalData
        {
            public Color4 FogColor;
            public Vector3 CameraPosition;
            public float CameraNear;
            public float CameraFar;
            public float FogStart;
            public float FogEnd;
            public float Time;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct PassData
        {
            public uint PassOrder;
            public uint Mode;
        };

        public static Camera main { get; set; }
        public static Camera current { get; private set; }
        public static int MSAA => 1;

        public D3D11.DeviceContext renderContext = null;
        public CameraRenderMode renderMode = CameraRenderMode.PerFrame;

        public Color4 ClearColor = new Color4(0.73f, 0.82f, 0.99f, 1f);

        public D3D11.Texture2D renderTexture { get; private set; }
        public D3D11.Texture2D depthTexture { get; private set; }

        public D3D11.RenderTargetView renderTextureView;
        public D3D11.Texture2D[] deferredRenderTextures;
        public D3D11.RenderTargetView[] renderTargetViews;
        public D3D11.ShaderResourceView[] shaderResourceViews;
        public D3D11.DepthStencilView depthStencilView;

        public D3D11.RasterizerState rasterizerState;
        public D3D11.BlendState blendState;
        public D3D11.DepthStencilState depthStencilState;

        public Shader debugPass;

        private D3D11.Buffer globalDataBuffer;
        private GlobalData globalData;

        private D3D11.Buffer passDataBuffer;
        private PassData passData;

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

        public void CreateResources()
        {
            debugPass = Shader.FromFile("shaders/passes/debug.hlsl");

            D3D11.RasterizerStateDescription rasterizerDesc = new D3D11.RasterizerStateDescription()
            {
                CullMode = D3D11.CullMode.Back,
                FillMode = D3D11.FillMode.Solid,
                IsDepthClipEnabled = false
            };

            rasterizerState = new D3D11.RasterizerState(Global.device, rasterizerDesc);

            D3D11.BlendStateDescription blendDesc = new D3D11.BlendStateDescription()
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true
            };

            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].SourceBlend = D3D11.BlendOption.One;
            blendDesc.RenderTarget[0].DestinationBlend = D3D11.BlendOption.One;
            blendDesc.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = D3D11.BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = D3D11.BlendOption.One;
            blendDesc.RenderTarget[0].AlphaBlendOperation = D3D11.BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All;

            blendState = new D3D11.BlendState(Global.device, blendDesc);

            D3D11.DepthStencilStateDescription depthStencilStateDesc = new D3D11.DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = D3D11.DepthWriteMask.Zero,
                DepthComparison = D3D11.Comparison.LessEqual
            };

            depthStencilState = new D3D11.DepthStencilState(Global.device, depthStencilStateDesc);
        }

        public void CreateBuffers(D3D11.Texture2D renderTexture, int MSAA)
        {
            this.renderTexture = renderTexture;

            renderTextureView = new D3D11.RenderTargetView(Global.device, renderTexture);

            deferredRenderTextures = new D3D11.Texture2D[3];
            renderTargetViews = new D3D11.RenderTargetView[deferredRenderTextures.Length];
            shaderResourceViews = new D3D11.ShaderResourceView[deferredRenderTextures.Length];

            D3D11.Texture2DDescription deferredTexDesc = new D3D11.Texture2DDescription()
            {
                Format = Format.R32G32B32A32_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = Global.window.ClientSize.Width,
                Height = Global.window.ClientSize.Height,
                SampleDescription = new SampleDescription(MSAA, 0),
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription()
            {
                Format = deferredTexDesc.Format,
                Dimension = D3D11.RenderTargetViewDimension.Texture2D
            };

            renderTargetViewDesc.Texture2D.MipSlice = 0;

            D3D11.ShaderResourceViewDescription shaderViewDesc = new D3D11.ShaderResourceViewDescription()
            {
                Format = deferredTexDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };

            shaderViewDesc.Texture2D.MostDetailedMip = 0;
            shaderViewDesc.Texture2D.MipLevels = 1;

            for (int i = 0; i < deferredRenderTextures.Length; i++)
            {
                deferredRenderTextures[i] = new D3D11.Texture2D(Global.device, deferredTexDesc);
                renderTargetViews[i] = new D3D11.RenderTargetView(Global.device, deferredRenderTextures[i], renderTargetViewDesc);
                shaderResourceViews[i] = new D3D11.ShaderResourceView(Global.device, deferredRenderTextures[i], shaderViewDesc);
            }

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

            globalDataBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref globalData, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
            //passDataBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref passData, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
        }

        public void ForceDispose(bool disposeResources)
        {
            for (int i = 0; i < shaderResourceViews.Length; i++)
            {
                dispose(shaderResourceViews[i]);
            }

            for (int i = 0; i < renderTargetViews.Length; i++)
            {
                dispose(renderTargetViews[i]);
            }

            for (int i = 0; i < deferredRenderTextures.Length; i++)
            {
                dispose(deferredRenderTextures[i]);
            }

            if (disposeResources)
            {
                dispose(debugPass);
                dispose(rasterizerState);
                dispose(blendState);
                dispose(depthStencilState);
            }

            dispose(globalDataBuffer);
            dispose(passDataBuffer);
            dispose(renderTextureView);
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
            ForceDispose(true);
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
            context.OutputMerger.SetRenderTargets(depthStencilView, renderTargetViews);

            // Clear buffers
            for (int i = 0; i < renderTargetViews.Length; i++)
            {
                context.ClearRenderTargetView(renderTargetViews[i], Color4.Black);
            }
            context.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1f, 0);

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.AllowRender && beh.activeSelf) beh.Render(context); });

            context.OutputMerger.SetRenderTargets(depthStencilView, renderTextureView);

            context.ClearRenderTargetView(renderTextureView, Color4.Black);
            context.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1f, 0);

            UpdateConstantBuffers(context);

            context.Rasterizer.State = rasterizerState;
            context.OutputMerger.SetBlendState(blendState, Color4.White, 0xFFFFFFFF);
            context.OutputMerger.SetDepthStencilState(depthStencilState, 0);

            debugPass.StartShaders(context);

            context.PixelShader.SetShaderResources(0, shaderResourceViews);
            context.PixelShader.SetConstantBuffer(0, globalDataBuffer);
            context.PixelShader.SetConstantBuffer(1, passDataBuffer);

            context.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(null, Utilities.SizeOf<Mesh.VertexData>(), 0));
            context.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
            context.Draw(3, 0);

            if (current == main)
            {
                Global.deviceContext2D.SaveDrawingState(Global.stateBlock);
                Global.deviceContext2D.BeginDraw();
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh != null && beh.AllowRender && beh.activeSelf) beh.Render2D(Global.deviceContext2D); });
                Global.deviceContext2D.EndDraw();
                Global.deviceContext2D.RestoreDrawingState(Global.stateBlock);
            }

            current = null;
        }

        private void UpdateConstantBuffers(D3D11.DeviceContext context)
        {
            globalData = new GlobalData
            {
                FogColor = ClearColor,
                CameraPosition = transform.position,
                CameraNear = clippingNear,
                CameraFar = clippingFar,
                Time = Time.time,
                FogStart = RenderSettings.FogStart,
                FogEnd = RenderSettings.FogEnd
            };

            passData = new PassData()
            {
                PassOrder = 0,
                Mode = 0
            };

            DataStream stream = null;
            DataBox box = context.MapSubresource(globalDataBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            using (stream)
            {
                stream.Write(globalData);
                context.UnmapSubresource(globalDataBuffer, 0);
            }

            /*box = context.MapSubresource(passDataBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            using (stream)
            {
                stream.Write(passData);
                context.UnmapSubresource(passDataBuffer, 0);
            }*/
        }
    }
}