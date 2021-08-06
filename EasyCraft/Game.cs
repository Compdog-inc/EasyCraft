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
using System.Windows.Forms;

namespace EasyCraft
{
    public class Game : IDisposable
    {
        private long lastUpdateTicks;

        public Game(string[] args)
        {
            App.ProcessArgs = CommandLine.ParseArguments(args, '/', '-');

            Global.game = this;
            Global.CurrentProcess = System.Diagnostics.Process.GetCurrentProcess();
            Global.window = new RenderForm("Easy Craft")
            {
                AllowUserResizing = true,
                StartPosition = FormStartPosition.CenterScreen,
                Icon = Properties.Resources.app_icon
            };

            Screen windowScreen;
            if (App.ProcessArgs.HasFlag("m"))
            {
                if(int.TryParse(App.ProcessArgs.GetFlag("m"), out int screenId))
                    windowScreen = Screen.AllScreens[screenId];
                else
                    windowScreen = Screen.FromControl(Global.window);
            } else
                windowScreen = Screen.FromControl(Global.window);

            float aspect = (float)windowScreen.WorkingArea.Height / (float)windowScreen.WorkingArea.Width;
            int width = (int)(windowScreen.WorkingArea.Width * 0.8f);
            int height = (int)(width * aspect);

            Global.window.StartPosition = FormStartPosition.Manual;
            Global.window.Location = new System.Drawing.Point(windowScreen.WorkingArea.X + windowScreen.WorkingArea.Width / 2 - width / 2, windowScreen.WorkingArea.Y + windowScreen.WorkingArea.Height / 2 - height / 2);
            Global.window.Width = width;
            Global.window.Height = height;

            Global.window.FormClosing += (s, e) =>
            {
                e.Cancel = !App.IsQuitting;
                App.Quit();
            };
            Global.window.UserResized += (s, e) => App.WindowResized = true;

            InitializeDeviceResources();
            InitializeBuildManager();
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
                SampleDescription = new SampleDescription(Camera.MSAA, 0),
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
            D3D11.DeviceDebug debug = new D3D11.DeviceDebug(Global.device);
            D3D11.InfoQueue infoQueue = debug.QueryInterface<D3D11.InfoQueue>();

            infoQueue.SetBreakOnSeverity(D3D11.MessageSeverity.Corruption, true);
            infoQueue.SetBreakOnSeverity(D3D11.MessageSeverity.Error, true);

            D3D11.MessageId[] hide = new D3D11.MessageId[]
            {
                D3D11.MessageId.MessageIdSetPrivateDataChangingparams
            };

            D3D11.InfoQueueFilter filter = new D3D11.InfoQueueFilter();
            filter.AllowList = new D3D11.InfoQueueFilterDescription();
            filter.DenyList = new D3D11.InfoQueueFilterDescription()
            {
                Ids = hide
            };

            infoQueue.AddStorageFilterEntries(filter);

            debug.ReportLiveDeviceObjects(D3D11.ReportingLevel.Summary | D3D11.ReportingLevel.Detail);

            infoQueue.Dispose();
            debug.Dispose();
#endif

            Global.swapChain = new SwapChain(Global.device.QueryInterface<Device>().Adapter.GetParent<Factory4>(), Global.device, swapChainDesc);
            Global.surface = Global.swapChain.GetBackBuffer<Surface>(0);
            Global.targetBitmap2D = new D2D1.Bitmap(Global.deviceContext2D, Global.surface, new D2D1.BitmapProperties(new D2D1.PixelFormat(Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Premultiplied)));
            Global.deviceContext2D.Target = Global.targetBitmap2D;

            CreateViews();
            CreateRenderStates();
        }

        private void InitializeBuildManager()
        {
            BuildManager.AddScene(new scenes.Game());
        }

        private void CreateViews()
        {
            Global.viewport = new Viewport(0, 0, Global.window.ClientSize.Width, Global.window.ClientSize.Height);
            Global.deviceContext.Rasterizer.SetViewport(Global.viewport);
        }

        public void UpdateMainCamera()
        {
            Camera.main.CreateResources();
            Camera.main.CreateBuffers(Global.swapChain.GetBackBuffer<D3D11.Texture2D>(0), Camera.MSAA);
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

        public void Run()
        {
            TryRun(()=>Input.Init());

            if (!SceneManager.LoadScene(0))
            {
                App.ShowAlert("Couldn't load scene.", "No Scene", true, AlertStyle.Error);
                return;
            }

            Foreach(Behavior.frameUpdateObjects, (beh) => {
                if (!beh._active && beh.active) TryRun(()=>beh.OnEnable());
                if (beh._active && !beh.active) TryRun(()=>beh.OnDisable());
                beh._active = beh.active;
            });

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(()=>beh.Awake()); });
            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(()=>beh.Start()); });

            lastUpdateTicks = DateTime.Now.Ticks;

            RenderLoop.Run(Global.window, RenderCallback);

            Foreach(Behavior.frameUpdateObjects, (beh) => {
                beh.active = false;
                beh._active = false;
                TryRun(() => beh.OnDisable());
            });

            if (!SceneManager.UnloadScene())
            {
                Debug.LogError("Couldn't unload scene. Destroying all objects.");
            }

            while (Behavior.objects.Count > 0)
            {
                TryRun(() => Behavior.objects[0].Destroy());
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

            Foreach(Behavior.frameUpdateObjects, (beh) => {
                if (!beh._active && beh.active) TryRun(() => beh.OnEnable());
                if (beh._active && !beh.active) TryRun(() => beh.OnDisable());
                beh._active = beh.active;
            });

            if (Time.timeSinceLastFixedDelta >= Time.fixedDeltaTime)
            {
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(()=>beh.FixedUpdate()); });
                Time.timeSinceLastFixedDelta = 0f;
            }

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(() => beh.Update()); });

            Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(() => beh.LateUpdate()); });

            Draw();

            TryRun(()=>Input.ResetAxis());

            if (App.IsQuitting)
            {
                Foreach(Behavior.frameUpdateObjects, (beh) => { if (beh.activeSelf) TryRun(()=>beh.OnApplicationQuit()); });
                if (App.IsQuitting)
                    Global.window.Close();
            }
            App.IsThreadDone = true;
        }

        private void Draw()
        {
            if (App.WindowResized)
            {
                Camera.main.ForceDispose(false);
                App.SafeDispose(Global.targetBitmap2D, this);
                App.SafeDispose(Global.surface, this);
                Global.deviceContext2D.Target = null;
                Global.deviceContext.Flush();
                Global.swapChain.ResizeBuffers(1, Global.window.ClientSize.Width, Global.window.ClientSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
                Global.surface = Global.swapChain.GetBackBuffer<Surface>(0);
                CreateViews();
                UpdateMainCamera();
                Global.targetBitmap2D = new D2D1.Bitmap(Global.deviceContext2D, Global.surface, new D2D1.BitmapProperties(new D2D1.PixelFormat(Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Premultiplied)));
                Global.deviceContext2D.Target = Global.targetBitmap2D;
                App.WindowResized = false;
            }

            MeshRenderer.RenderedInstances = 0;
            // Render
            Foreach(Behavior.frameRenderObjects, (beh) => { if (beh.activeSelf) TryRun(()=>beh.Render(Global.deviceContext)); });

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
                    UpdateMainCamera();
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
