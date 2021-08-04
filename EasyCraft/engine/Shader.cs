using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Collections.Generic;
using System.IO;

namespace EasyCraft.engine
{
    public class Shader : IDisposable
    {
        private class StandardIncludeHandler : CppObject, Include
        {
            public StandardIncludeHandler() : base(new IntPtr(1)) { }

            public void Close(Stream stream) { }

            public Stream Open(IncludeType type, string fileName, Stream parentStream) { throw new NotImplementedException(); }
        }

        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;
        private D3D11.InputLayout inputLayout;

        protected byte[] vertexBytecode;
        protected byte[] pixelBytecode;

        public Shader(Shader source)
        {
            this.vertexBytecode = source.vertexBytecode;
            this.pixelBytecode = source.pixelBytecode;
            this.vertexShader = new D3D11.VertexShader(Global.device, source.vertexBytecode);
            this.pixelShader = new D3D11.PixelShader(Global.device, source.pixelBytecode);
            this.inputLayout = GetInputLayoutFromBytecode(Global.device, source.vertexBytecode);
        }

        public Shader(D3D11.VertexShader vertexShader, D3D11.PixelShader pixelShader, D3D11.InputLayout inputLayout)
        {
            this.vertexShader = vertexShader;
            this.pixelShader = pixelShader;
            this.inputLayout = inputLayout;
        }

        public static Shader FromBytecode(byte[] vertexShader, byte[] pixelShader)
        {
            Shader shader = new Shader(
                new D3D11.VertexShader(Global.device, vertexShader),
                new D3D11.PixelShader(Global.device, pixelShader),
                GetInputLayoutFromBytecode(Global.device, vertexShader)
                );
            shader.vertexBytecode = vertexShader;
            shader.pixelBytecode = pixelShader;
            return shader;
        }

        public static Shader FromFile(string shader)
        {
            try
            {
                return FromFileEx(shader);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while compiling shader from file: " + shader + ". " + e.ToString());
                try
                {
                    return FromFileEx("shaders/error.hlsl");
                } catch
                {
                }
            }
            return null;
        }

        private static Shader FromFileEx(string shader)
        {
            D3D11.VertexShader vs;
            D3D11.PixelShader ps;
            D3D11.InputLayout il;
            byte[] vertexBytecode = null;
            byte[] pixelBytecode = null;

            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            flags |= ShaderFlags.OptimizationLevel3;
#endif

            using (var byteCode = ShaderBytecode.CompileFromFile(shader, "VSMain", "vs_4_0", flags, EffectFlags.None, null, new StandardIncludeHandler()))
            {
                vertexBytecode = byteCode;
                vs = new D3D11.VertexShader(Global.device, byteCode);
                il = GetInputLayoutFromBytecode(Global.device, byteCode);
            }
            using (var byteCode = ShaderBytecode.CompileFromFile(shader, "PSMain", "ps_4_0", flags, EffectFlags.None, null, new StandardIncludeHandler()))
            {
                pixelBytecode = byteCode;
                ps = new D3D11.PixelShader(Global.device, byteCode);
            }

            Shader s = new Shader(vs, ps, il)
            {
                vertexBytecode = vertexBytecode,
                pixelBytecode = pixelBytecode
            };
            return s;
        }

        private static D3D11.InputLayout GetInputLayoutFromBytecode(D3D11.Device d3dDevice, byte[] shaderBytecode)
        {
            ShaderReflection shaderReflection = new ShaderReflection(shaderBytecode);
            ShaderDescription shaderDesc = shaderReflection.Description;
            List<D3D11.InputElement> inputElements = new List<D3D11.InputElement>();

            for (int i = 0; i < shaderDesc.InputParameters; i++)
            {
                ShaderParameterDescription paramDesc = shaderReflection.GetInputParameterDescription(i);

                D3D11.InputElement element = new D3D11.InputElement();
                element.SemanticName = paramDesc.SemanticName;
                element.SemanticIndex = paramDesc.SemanticIndex;
                element.Slot = 0;
                element.AlignedByteOffset = D3D11.InputElement.AppendAligned;
                element.Classification = D3D11.InputClassification.PerVertexData;
                element.InstanceDataStepRate = 0;

                int mask = (int)paramDesc.UsageMask;
                if (mask == 1)
                {
                    if (paramDesc.ComponentType == RegisterComponentType.UInt32) element.Format = Format.R32_UInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.SInt32) element.Format = Format.R32_SInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.Float32) element.Format = Format.R32_Float;
                }
                else if (mask <= 3)
                {
                    if (paramDesc.ComponentType == RegisterComponentType.UInt32) element.Format = Format.R32G32_UInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.SInt32) element.Format = Format.R32G32_SInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.Float32) element.Format = Format.R32G32_Float;
                }
                else if (mask <= 7)
                {
                    if (paramDesc.ComponentType == RegisterComponentType.UInt32) element.Format = Format.R32G32B32_UInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.SInt32) element.Format = Format.R32G32B32_SInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.Float32) element.Format = Format.R32G32B32_Float;
                }
                else if (mask <= 15)
                {
                    if (paramDesc.ComponentType == RegisterComponentType.UInt32) element.Format = Format.R32G32B32A32_UInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.SInt32) element.Format = Format.R32G32B32A32_SInt;
                    else if (paramDesc.ComponentType == RegisterComponentType.Float32) element.Format = Format.R32G32B32A32_Float;
                }

                inputElements.Add(element);
            }

            ShaderSignature signature = ShaderSignature.GetInputSignature(shaderBytecode);
            D3D11.InputLayout layout = new D3D11.InputLayout(Global.device, signature, inputElements.ToArray());
            signature.Dispose();
            shaderReflection.Dispose();
            return layout;
        }

        public void StartShaders(D3D11.DeviceContext context)
        {
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = inputLayout;
        }

        public void Dispose()
        {
            App.SafeDispose(inputLayout, this);
            App.SafeDispose(vertexShader, this);
            App.SafeDispose(pixelShader, this);
        }
    }
}
