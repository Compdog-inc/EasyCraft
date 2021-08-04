using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using EasyCraft.engine;
using System.Collections.Generic;
using D2D1 = SharpDX.Direct2D1;

namespace EasyCraft
{
    public class Game : IDisposable
    {
        private int MSAA = 1;
        private long lastUpdateTicks;
        private Camera outputCamera;

        public Game()
        {
            Global.CurrentProcess = System.Diagnostics.Process.GetCurrentProcess();
            Global.window = new RenderForm("Easy Craft")
            {
                AllowUserResizing = true,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                ClientSize = new System.Drawing.Size(1920, 1080),
                Icon = System.Drawing.SystemIcons.Application,
            };

            Global.window.FormClosing += (s, e) =>
            {
                e.Cancel = !App.IsQuitting;
                App.Quit();
            };
            Global.window.UserResized += (s, e) => App.WindowResized = true;

            outputCamera = new Camera();

            InitializeDeviceResources();
        }

        private void InitializeDeviceResources()
        {
            Global.factory2D = new D2D1.Factory();
            Global.imagingFactory = new SharpDX.WIC.ImagingFactory();
            Global.dwFactory = new SharpDX.DirectWrite.Factory();

            ModeDescription backBufferDesc = new ModeDescription(Global.window.ClientSize.Width, Global.window.ClientSize.Height, new Rational(60, 1), Format.B8G8R8A8_UNorm);
            
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(MSAA, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = Global.window.Handle,
                IsWindowed = true,
                Flags = SwapChainFlags.None
            };

            D3D11.DeviceCreationFlags deviceFlags = D3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            deviceFlags |= D3D11.DeviceCreationFlags.Debug;
#endif
            D3D11.Device device = new D3D11.Device(DriverType.Hardware, deviceFlags);
            Global.device = new D3D11.Device3(device.NativePointer);
            Global.deviceContext = Global.device.ImmediateContext;
            Global.device2D = new D2D1.Device(Global.device.QueryInterface<Device>());
            Global.deviceContext2D = new D2D1.DeviceContext(Global.device2D, D2D1.DeviceContextOptions.None);
            Global.stateBlock = new D2D1.DrawingStateBlock(Global.factory2D);
#if DEBUG
            Global.debug = new D3D11.DeviceDebug(Global.device);
#endif
            Global.swapChain = new SwapChain(Global.device.QueryInterface<Device>().Adapter.GetParent<Factory4>(), Global.device, swapChainDesc);
            Global.surface = Global.swapChain.GetBackBuffer<Surface>(0);
            Global.targetBitmap2D = new D2D1.Bitmap(Global.deviceContext2D, Global.surface, new D2D1.BitmapProperties(new D2D1.PixelFormat(Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Premultiplied)));
            Global.deviceContext2D.Target = Global.targetBitmap2D;
            CreateViews();

            CreateRenderStates();
        }

        private void CreateViews()
        {
            outputCamera.CreateBuffers(Global.swapChain.GetBackBuffer<D3D11.Texture2D>(0), MSAA);

            Global.viewport = new Viewport(0, 0, Global.window.ClientSize.Width, Global.window.ClientSize.Height);
            Global.deviceContext.Rasterizer.SetViewport(Global.viewport);
        }

        private void CreateRenderStates()
        {
            Global.cullBackState = new D3D11.RasterizerState(Global.device, new D3D11.RasterizerStateDescription()
            {
                CullMode = D3D11.CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = D3D11.FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            });

            Global.cullFrontState = new D3D11.RasterizerState(Global.device, new D3D11.RasterizerStateDescription()
            {
                CullMode = D3D11.CullMode.Front,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = D3D11.FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            });

            Global.cullNoneState = new D3D11.RasterizerState(Global.device, new D3D11.RasterizerStateDescription()
            {
                CullMode = D3D11.CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = D3D11.FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            });

            Global.depthStencilState = new D3D11.DepthStencilState(Global.device, new D3D11.DepthStencilStateDescription()
            {
                // Depth test parameters
                IsDepthEnabled = true,
                DepthWriteMask = D3D11.DepthWriteMask.All,
                DepthComparison = D3D11.Comparison.Less,

                // Stencil test parameters
                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                // Stencil operations if pixel is front-facing
                FrontFace = new D3D11.DepthStencilOperationDescription()
                {
                    FailOperation = D3D11.StencilOperation.Keep,
                    DepthFailOperation = D3D11.StencilOperation.Increment,
                    PassOperation = D3D11.StencilOperation.Keep,
                    Comparison = D3D11.Comparison.Always
                },

                // Stencil operations if pixel is back-facing
                BackFace = new D3D11.DepthStencilOperationDescription()
                {
                    FailOperation = D3D11.StencilOperation.Keep,
                    DepthFailOperation = D3D11.StencilOperation.Decrement,
                    PassOperation = D3D11.StencilOperation.Keep,
                    Comparison = D3D11.Comparison.Always
                }
            });
        }

        private bool TryRun(Action action)
        {
            try
            {
                action.Invoke();
            } catch(Exception e)
            {
                Debug.LogError(e.Message.TrimEnd() + "\n" + e.StackTrace.TrimEnd(), action.Target);
                return false;
            }
            return true;
        }

        public void InitScene()
        {
            GenerationScreen genScreen = new GenerationScreen() { name = "Generation Screen" };
            DirectionalLight sun = new DirectionalLight() { name = "Sun" };
            sun.transform.position = new Vector3(-1.8f, 1.8f, 1.8f);
            DebugScreen debugScreen = new DebugScreen() { name = "Debug Screen" };
            World world = new World() { name = "World", generationScreen = genScreen };
            PauseScreen pauseScreen = new PauseScreen() { name = "Pause Screen", active = false };
            Player player = new Player() { name = "Player", pauseScreen = pauseScreen };
            outputCamera.name = "Output Camera";
            outputCamera.transform.SetParent(player.transform);
            world.player = player;
        }

        public void Run()
        {
            TryRun(()=>Input.Init());

            TryRun(()=>InitScene());
            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(()=>beh.Awake()); });
            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(()=>beh.Start()); });

            lastUpdateTicks = DateTime.Now.Ticks;

            RenderLoop.Run(Global.window, RenderCallback);

            while (Behavior.objects.Count > 0)
            {
                TryRun(()=>Behavior.objects[0].Destroy());
            }
        }

        private void Foreach<T>(List<T> list, Action<T> action)
        {
            lock (list)
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
        }

        private void RenderCallback()
        {
            if (App.IsThreadLocked) return;
            App.IsThreadDone = false;
            long currentTicks = DateTime.Now.Ticks;
            Time.deltaTime = (currentTicks - lastUpdateTicks) / 10000000f;
            lastUpdateTicks = currentTicks;
            Time.time += Time.deltaTime;
            Time.timeSinceLastFixedDelta += Time.deltaTime;

            if (Time.timeSinceLastFixedDelta >= Time.fixedDeltaTime)
            {
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(()=>beh.FixedUpdate()); });
                Time.timeSinceLastFixedDelta = 0f;
            }

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(() => beh.Update()); });

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(() => beh.LateUpdate()); });

            Draw();

            TryRun(()=>Input.ResetAxis());

            if (App.IsQuitting)
            {
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.active) TryRun(()=>beh.OnApplicationQuit()); });
                if (App.IsQuitting)
                    Global.window.Close();
            }
            App.IsThreadDone = true;
        }

        private void Draw()
        {
            if (App.WindowResized)
            {
                outputCamera.ForceDispose();
                App.SafeDispose(Global.targetBitmap2D, this);
                App.SafeDispose(Global.surface, this);
                Global.deviceContext2D.Target = null;
                Global.deviceContext.Flush();
                Global.swapChain.ResizeBuffers(1, Global.window.ClientSize.Width, Global.window.ClientSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
                Global.surface = Global.swapChain.GetBackBuffer<Surface>(0);
                CreateViews();
                Global.targetBitmap2D = new D2D1.Bitmap(Global.deviceContext2D, Global.surface, new D2D1.BitmapProperties(new D2D1.PixelFormat(Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Premultiplied)));
                Global.deviceContext2D.Target = Global.targetBitmap2D;
                App.WindowResized = false;
            }

            MeshRenderer.RenderedInstances = 0;
            // Render
            Foreach(Behavior.frameRenderObjects, (beh) => { if (beh.active) TryRun(()=>beh.Render(Global.deviceContext)); });

            try
            {
                // Present buffer
                Global.swapChain.Present(0, PresentFlags.None);
            } catch (SharpDXException ex)
            {
                if ((ex.ResultCode == ResultCode.DeviceRemoved) || (ex.ResultCode == ResultCode.DeviceReset))
                {
                    Destroy(false);
                    InitializeDeviceResources();
                }
                else
                {
                    throw;
                }
            }
        }

        public void Destroy(bool window)
        {
            App.SafeDispose(Global.cullBackState, this);
            App.SafeDispose(Global.cullFrontState, this);
            App.SafeDispose(Global.cullNoneState, this);
            App.SafeDispose(Global.depthStencilState, this);
            outputCamera?.ForceDispose();
#if DEBUG
            App.SafeDispose(Global.debug, this);
#endif
            App.SafeDispose(Global.imagingFactory, this);
            App.SafeDispose(Global.dwFactory, this);
            App.SafeDispose(Global.targetBitmap2D, this);
            App.SafeDispose(Global.surface, this);
            App.SafeDispose(Global.swapChain, this);
            App.SafeDispose(Global.device, this);
            App.SafeDispose(Global.device2D, this);
            App.SafeDispose(Global.deviceContext, this);
            App.SafeDispose(Global.deviceContext2D, this);
            App.SafeDispose(Global.factory2D, this);
            App.SafeDispose(Global.stateBlock, this);
            if (window)
            {
                App.SafeDispose(Global.window, this);
                App.SafeDispose(Global.CurrentProcess, this);
            }
        }

        public void Dispose()
        {
            Destroy(true);
        }
    }
}
