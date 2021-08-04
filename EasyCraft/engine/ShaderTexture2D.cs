using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using SharpDX.WIC;

namespace EasyCraft.engine
{
    public class ShaderTexture2D : IDisposable
    {
        private D3D11.Texture2D texture;
        private D3D11.ShaderResourceView textureView;
        private D3D11.SamplerState sampler;
        private int arrayDepth = 0;

        public void SetTexture(D3D11.Texture2D _texture, int mipLevels = 8, int mipMin = 0)
        {
            SetTexture(_texture, new D3D11.SamplerStateDescription()
            {
                ComparisonFunction = D3D11.Comparison.Always,
                MipLodBias = 0,
                BorderColor = Color4.Black,
                AddressU = D3D11.TextureAddressMode.Clamp,
                AddressV = D3D11.TextureAddressMode.Clamp,
                AddressW = D3D11.TextureAddressMode.Clamp,
                Filter = D3D11.Filter.MinMagMipPoint,
                MinimumLod = mipMin,
                MaximumLod = mipLevels,
                MaximumAnisotropy = 16
            });
        }

        public void SetTextureArray(D3D11.Texture2D _texture, int arrayDepth, int mipLevels = 8, int mipMin = 0)
        {
            SetTextureArray(_texture, arrayDepth, new D3D11.SamplerStateDescription()
            {
                ComparisonFunction = D3D11.Comparison.Always,
                MipLodBias = 0,
                BorderColor = Color4.Black,
                AddressU = D3D11.TextureAddressMode.Clamp,
                AddressV = D3D11.TextureAddressMode.Clamp,
                AddressW = D3D11.TextureAddressMode.Clamp,
                Filter = D3D11.Filter.MinMagMipPoint,
                MinimumLod = mipMin,
                MaximumLod = mipLevels,
                MaximumAnisotropy = 16
            });
        }

        public void SetTexture(D3D11.Texture2D _texture, D3D11.SamplerStateDescription description)
        {
            texture = _texture;
            textureView = new D3D11.ShaderResourceView(Global.device, texture, new D3D11.ShaderResourceViewDescription()
            {
                Format = texture.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new D3D11.ShaderResourceViewDescription.Texture2DResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = -1
                }
            });
            Global.deviceContext.GenerateMips(textureView);
            UpdateSampler(description);
        }

        public void SetTextureArray(D3D11.Texture2D _texture, int arrayDepth, D3D11.SamplerStateDescription description)
        {
            texture = _texture;
            this.arrayDepth = arrayDepth;
            textureView = new D3D11.ShaderResourceView(Global.device, texture, new D3D11.ShaderResourceViewDescription()
            {
                Format = texture.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new D3D11.ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    ArraySize = arrayDepth,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0,
                    MipLevels = -1
                }
            });
            Global.deviceContext.GenerateMips(textureView);
            UpdateSampler(description);
        }

        public void UpdateSampler(D3D11.SamplerStateDescription description)
        {
            if (sampler != null) sampler.Dispose();
            sampler = new D3D11.SamplerState(Global.device, description);
        }

        public void StartTexture(D3D11.DeviceContext context, int slot)
        {
            context.PixelShader.SetSampler(slot, sampler);
            context.PixelShader.SetShaderResource(slot, textureView);
        }
        
        public static ShaderTexture2D FromFile(string filename, int mipLevels = 8, int mipMin = 0)
        {
            ShaderTexture2D texture = new ShaderTexture2D();
            BitmapSource source = TextureLoader.LoadBitmap(Global.imagingFactory, filename);
            texture.SetTexture(TextureLoader.CreateTexture2DFromBitmap(source, Global.device, Global.deviceContext, new D3D11.Texture2DDescription1()
            {
                ArraySize = 1,
                Usage = D3D11.ResourceUsage.Default,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 0,
                OptionFlags = D3D11.ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource
            }), mipLevels, mipMin);
            source.Dispose();
            return texture;
        }

        public static ShaderTexture2D FromFileArray(string[] filenames, int mipLevels = 8, int mipMin = 0)
        {
            ShaderTexture2D texture = new ShaderTexture2D();
            BitmapSource[] sources = new BitmapSource[filenames.Length];

            for(int i = 0; i < sources.Length; i++)
                sources[i] = TextureLoader.LoadBitmap(Global.imagingFactory, filenames[i]);

            texture.SetTextureArray(TextureLoader.CreateTexture2DArrayFromBitmaps(sources, Global.device, Global.deviceContext, new D3D11.Texture2DDescription()
            {
                ArraySize = sources.Length,
                Usage = D3D11.ResourceUsage.Default,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 0,
                OptionFlags = D3D11.ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource
            }), filenames.Length, mipLevels, mipMin);
            for (int i = 0; i < sources.Length; i++) sources[i].Dispose();
            return texture;
        }

        public void Dispose()
        {
            App.SafeDispose(textureView, this);
            App.SafeDispose(texture, this);
            App.SafeDispose(sampler, this);
        }
    }
}
