using System;
using D2D1 = SharpDX.Direct2D1;
using EasyCraft.engine;
using DW = SharpDX.DirectWrite;
using SharpDX;
using SharpDX.Mathematics.Interop;
using WIN = System.Windows.Forms;
using EasyCraft.engine.extensions;
using System.Collections.Generic;

namespace EasyCraft
{
    public class PauseScreen : Behavior
    {
        private DW.TextFormat textFormat;
        private D2D1.Brush textBrush;
        private D2D1.Brush tintBrush;

        private Vector2 screenSize;
        private Vector2 prevScreenSize;

        public override void Awake()
        {
            textFormat = new DW.TextFormat(Global.dwFactory, "Segoe UI", 32f);
            textBrush = new D2D1.SolidColorBrush(Global.deviceContext2D, new RawColor4(1, 1, 1, 1));
            tintBrush = new D2D1.SolidColorBrush(Global.deviceContext2D, new RawColor4(0, 0, 0, 0.5f));
        }

        public override void Start()
        {
            screenSize = new Vector2(Global.targetBitmap2D.Size.Width, Global.targetBitmap2D.Size.Height);
            prevScreenSize = screenSize;
        }

        public override void Update()
        {
            prevScreenSize = screenSize;
            screenSize = new Vector2(Global.targetBitmap2D.Size.Width, Global.targetBitmap2D.Size.Height);
        }

        public override void Render2D(D2D1.DeviceContext context)
        {
            context.FillRectangle(new RawRectangleF(0, 0, screenSize.X, screenSize.Y), tintBrush);
        }

        public override void OnDestroy()
        {
            dispose(textBrush);
            dispose(textFormat);
            dispose(tintBrush);
        }
    }
}
