using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;
using EasyCraft.engine.extensions;

namespace EasyCraft.engine
{
    public enum CullMode
    {
        None,
        Back,
        Front
    }

    public class Material : IDisposable
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
        private struct LightSource
        {
            public Vector4 lightViewPosition;
            public Color4 lightColor;
        };

        public Shader shader;
        public List<ShaderTexture2D> textures = new List<ShaderTexture2D>();
        public bool AlphaBlendEnabled = false;
        public bool ReceiveFog = true;
        public CullMode CullMode = CullMode.Back;

        private D3D11.Buffer globalDataBuffer;
        private GlobalData globalData;

        private D3D11.Buffer lightSourceBuffer;
        private LightSource lightSource;

        public Material() {
            globalDataBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref globalData, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
            lightSourceBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref lightSource, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
        }

        public Material(Material source)
        {
            if (source.shader != null)
                shader = new Shader(source.shader);
            textures = new List<ShaderTexture2D>(source.textures);
            ReceiveFog = source.ReceiveFog;
            globalDataBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref globalData, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
            lightSourceBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref lightSource, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
        }

        private void UpdateConstantBuffers(D3D11.DeviceContext context)
        {
            globalData = new GlobalData
            {
                FogColor = Camera.current.ClearColor,
                CameraPosition = Camera.current.transform.position,
                CameraNear = Camera.current.clippingNear,
                CameraFar = Camera.current.clippingFar,
                Time = Time.time
            };

            if (ReceiveFog)
            {
                globalData.FogStart = RenderSettings.FogStart;
                globalData.FogEnd = RenderSettings.FogEnd;
            }
            else
                globalData.FogStart = globalData.FogEnd = -1;

            lightSource = new LightSource()
            {
                lightViewPosition = Vector4.Transform(DirectionalLight.Current.transform.position.ToV4(1), Camera.current.ViewMatrix),
                lightColor = new Color4(1, 1, 0.7f, 4.5f)
            };

            DataStream stream = null;
            DataBox box = context.MapSubresource(globalDataBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            using (stream)
            {
                stream.Write(globalData);
                context.UnmapSubresource(globalDataBuffer, 0);
            }

            box = context.MapSubresource(lightSourceBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            using (stream)
            {
                stream.Write(lightSource);
                context.UnmapSubresource(lightSourceBuffer, 0);
            }
        }

        private void SetBlendState(D3D11.DeviceContext context)
        {
            D3D11.BlendStateDescription blendStateDescription = new D3D11.BlendStateDescription
            {
                AlphaToCoverageEnable = false,
            };

            blendStateDescription.RenderTarget[0].IsBlendEnabled = true;
            blendStateDescription.RenderTarget[0].SourceBlend = D3D11.BlendOption.SourceAlpha;
            blendStateDescription.RenderTarget[0].DestinationBlend = D3D11.BlendOption.InverseSourceAlpha;
            blendStateDescription.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
            blendStateDescription.RenderTarget[0].SourceAlphaBlend = D3D11.BlendOption.Zero;
            blendStateDescription.RenderTarget[0].DestinationAlphaBlend = D3D11.BlendOption.Zero;
            blendStateDescription.RenderTarget[0].AlphaBlendOperation = D3D11.BlendOperation.Add;
            blendStateDescription.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All;

            context.OutputMerger.BlendState = new D3D11.BlendState(Global.device, blendStateDescription);
        }

        public void Start(D3D11.DeviceContext context)
        {
            shader?.StartShaders(context);
            for (int i = 0; i < textures.Count; i++) textures[i].StartTexture(context, i);
            context.Rasterizer.State = CullMode == CullMode.None ? Global.cullNoneState : CullMode == CullMode.Back ? Global.cullBackState : CullMode == CullMode.Front ? Global.cullFrontState : null;
            context.OutputMerger.SetDepthStencilState(Global.depthStencilState);

            UpdateConstantBuffers(context);
            SetBlendState(context);

            context.PixelShader.SetConstantBuffer(0, globalDataBuffer);
            context.PixelShader.SetConstantBuffer(1, lightSourceBuffer);
        }

        public void Dispose()
        {
            App.SafeDispose(shader, this);
            App.SafeDispose(globalDataBuffer, this);
            App.SafeDispose(lightSourceBuffer, this);
            foreach (ShaderTexture2D tex in textures) App.SafeDispose(tex, this);
        }
    }
}