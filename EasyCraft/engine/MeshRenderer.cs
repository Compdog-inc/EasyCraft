using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Runtime.InteropServices;

namespace EasyCraft.engine
{
    public enum TriangleSize
    {
        UShort,
        UInt
    }

    public class MeshRenderer : Behavior
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Transformations
        {
            public Matrix modelTransform;
            public Matrix viewTransform;
            public Matrix projTransform;

            public override string ToString()
            {
                return $"{{Model: {modelTransform}, View: {viewTransform}, Proj: {projTransform}}}";
            }
        }
        public Mesh mesh { get => _mesh; set { _mesh = value; meshDirty = true; } }
        private Mesh _mesh;
        private bool meshDirty = false;

        public Material material;
        public TriangleSize triangleSize = TriangleSize.UShort;

        private D3D11.Buffer vertexBuffer;
        private D3D11.Buffer indexBuffer;
        private D3D11.Buffer transformationsBuffer;
        private Transformations transformations;

        public static int RenderedInstances { get; set; } = 0;

        public MeshRenderer() : base()
        {
            transformationsBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.ConstantBuffer, ref transformations, 0, D3D11.ResourceUsage.Dynamic, D3D11.CpuAccessFlags.Write);
        }

        private void UpdateConstantBuffer(D3D11.DeviceContext context)
        {
            DataStream stream = null;
            DataBox box = context.MapSubresource(transformationsBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            using (stream)
            {
                stream.Write(transformations);
                context.UnmapSubresource(transformationsBuffer, 0);
            }
        }

        private void UpdateMeshBuffers(D3D11.DeviceContext context)
        {
            if (vertexBuffer != null) vertexBuffer.Dispose();
            vertexBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.VertexBuffer, mesh.GetVertexData());

            if (indexBuffer != null) indexBuffer.Dispose();
            if (triangleSize == TriangleSize.UInt)
                indexBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.IndexBuffer, mesh.GetTrianglesUInt());
            else
                indexBuffer = D3D11.Buffer.Create(Global.device, D3D11.BindFlags.IndexBuffer, mesh.GetTrianglesUShort());
        }

        public override void Render(D3D11.DeviceContext context)
        {
            if (mesh == null)
                return;
            if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
                return;

            if (meshDirty)
            {
                UpdateMeshBuffers(Global.deviceContext);
                meshDirty = false;
            }

            transformations = new Transformations
            {
                modelTransform = Matrix.Transpose(transform.WorldMatrix),
                viewTransform = Matrix.Transpose(Camera.current.ViewMatrix),
                projTransform = Matrix.Transpose(Camera.current.ProjectionMatrix)
            };

            UpdateConstantBuffer(context);

            material.Start(context);

            context.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(
                vertexBuffer, Utilities.SizeOf<Mesh.VertexData>(), 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, triangleSize == TriangleSize.UInt ? Format.R32_UInt : Format.R16_UInt, 0);
            context.VertexShader.SetConstantBuffer(0, transformationsBuffer);

            context.DrawIndexed(mesh.triangles.Length, 0, 0);

            RenderedInstances++;
        }

        public override void OnDestroy()
        {
            if (!dispose(material)) warn("Material not disposed");
            if (!dispose(transformationsBuffer)) warn("Transformations not disposed");
            if (!dispose(vertexBuffer)) warn("Vertex buffer not disposed");
            if (!dispose(indexBuffer)) warn("Index buffer not disposed");
        }
    }
}