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
    public class Behavior
    {
        internal static List<Behavior> frameUpdateObjects = new List<Behavior>();
        internal static List<Behavior> frameRenderObjects = new List<Behavior>();
        internal static List<Behavior> objects = new List<Behavior>();

        public string name { get; set; }

        public Behavior()
        {
            if (!objects.Contains(this))
                objects.Add(this);
            SubscribeUpdate();
        }

        protected void SubscribeUpdate()
        {
            if (!frameUpdateObjects.Contains(this))
                frameUpdateObjects.Add(this);
        }

        protected void UnsubscribeUpdate()
        {
            if (frameUpdateObjects.Contains(this))
                frameUpdateObjects.Remove(this);
        }

        protected void SubscribeRender()
        {
            if (!frameRenderObjects.Contains(this))
                frameRenderObjects.Add(this);
        }

        protected void UnsubscribeRender()
        {
            if (frameRenderObjects.Contains(this))
                frameRenderObjects.Remove(this);
        }

        protected void log(object obj)
        {
            Debug.Log(obj, this);
        }

        protected void warn(object obj)
        {
            Debug.LogWarning(obj, this);
        }

        protected void error(object obj)
        {
            Debug.LogError(obj, this);
        }

        protected bool dispose(IDisposable d)
        {
            log("Disposing " + d + ": " + name);
            return App.SafeDispose(d, this);
        }

        public bool activeSelf { get => active && _active; }

        public Transform transform = new Transform();
        public bool active = true;
        public bool AllowRender { get; protected set; } = true;

        internal bool _active = false;

        public void Destroy()
        {
            UnsubscribeUpdate();
            UnsubscribeRender();
            if (objects.Contains(this))
                objects.Remove(this);
            OnDestroy();
        }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnApplicationQuit() { }

        public virtual void Render(D3D11.DeviceContext context) { }
        public virtual void Render2D(D2D1.DeviceContext context) { }
    }
}
