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
    public abstract class Scene
    {
        public abstract string name { get; }

        public bool isLoading { get => _loading; }
        public bool isLoaded { get => _loaded; }
        public bool hasFailed { get => _failed; }

        public Exception failException { get => _failException; }

        private bool _loading = false;
        private bool _loaded = false;
        private bool _failed = false;
        private Exception _failException = null;

        public void InitUnLoad()
        {
            _failed = false;
            _failException = null;
            _loaded = false;
            _loading = false;
        }

        public void FailUnLoad(Exception e)
        {
            _failed = true;
            _failException = e;
            _loaded = false;
            _loading = false;
        }

        public void InitLoad()
        {
            _failed = false;
            _failException = null;
            _loaded = false;
            _loading = true;
        }

        public void EndLoad()
        {
            _failed = false;
            _failException = null;
            _loaded = true;
            _loading = false;
        }

        public void FailLoad(Exception e)
        {
            _failed = true;
            _failException = e;
            _loaded = false;
            _loading = false;
        }

        public abstract void CreateSceneObjects();
        public abstract void DestroySceneObjects();
    }
}
