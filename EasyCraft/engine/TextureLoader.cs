using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;

namespace EasyCraft.engine
{
    public class TextureLoader
    {
        public static BitmapSource LoadBitmap(ImagingFactory factory, string filename)
        {
            var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand
                );

            var result = new FormatConverter(factory);

            result.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            App.SafeDispose(bitmapDecoder);

            return result;
        }

        public static Texture2D CreateTexture2DFromBitmap(BitmapSource source, Device3 device, DeviceContext context, Texture2DDescription1 desc)
        {
            desc.Width = source.Size.Width;
            desc.Height = source.Size.Height;

            Texture2D t2d = null;
            int stride = source.Size.Width * 4;
            var buffer = new DataStream(source.Size.Height * stride, true, true);

            if (desc.MipLevels != 0)
            {
                // if we arent doing mip maps, then load the resource directly
                DataRectangle rect = new DataRectangle(buffer.DataPointer, stride);
                source.CopyPixels(stride, buffer);
                t2d = new Texture2D1(device, desc, rect); /// this creates a texture and populates it, we need to create a blank one then populate it.                
            }
            else
            {
                t2d = new Texture2D1(device, desc); /// this creates a texture and populates it, we need to create a blank one then populate it.
                source.CopyPixels(stride, buffer);
                DataBox box = new DataBox(buffer.DataPointer, stride, 1);

                context.UpdateSubresource(box, t2d, Resource.CalculateSubResourceIndex(0, 0, CountMips(source.Size.Width)));
            }
            App.SafeDispose(buffer);
            return t2d;
        }

        public static Texture2D CreateTexture2DArrayFromBitmaps(BitmapSource[] sources, Device3 device, DeviceContext context, Texture2DDescription desc)
        {
            if (sources.Length == 0) return null;
            desc.Width = sources[0].Size.Width;
            desc.Height = sources[0].Size.Height;

            Texture2D texture = new Texture2D(Global.device, desc);

            for (int i = 0; i < sources.Length; i++)
            {
                int stride = sources[i].Size.Width * 4;
                using (DataStream buffer = new DataStream(sources[i].Size.Height * stride, true, true))
                {
                    sources[i].CopyPixels(stride, buffer);
                    DataBox box = new DataBox(buffer.DataPointer, stride, 1);
                    context.UpdateSubresource(box, texture, Resource.CalculateSubResourceIndex(0, i, CountMips(sources[i].Size.Width)));
                }
            }
            return texture;
        }

        private static int CountMips(int width)
        {
            return (int)Math.Log(width, 2) + 1;
        }
    }
}