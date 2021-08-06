using System;
using D2D1 = SharpDX.Direct2D1;
using EasyCraft.engine;
using DW = SharpDX.DirectWrite;
using SharpDX;
using SharpDX.Mathematics.Interop;
using WIN = System.Windows.Forms;
using EasyCraft.engine.extensions;

namespace EasyCraft
{
    public class DebugScreen : Behavior
    {
        private DW.TextFormat textFormat;
        private D2D1.Brush textBrush;
        private D2D1.Brush backgroundBrush;

        private int frameCount = 0;
        private float fpsCounter = 0;
        private float fps = 0;
        private float fpsUpdateRate = 4.0f;
        private Vector3 playerChunkPos = new Vector3();
        private Vector3 chunkPos = new Vector3();
        private Vector3 rotation = new Vector3();

        private bool f3Down = false;
        private float f3CDownTime = 0;
        private int prevF3CDownTime = 0;

        private bool showDebug = false;

        public override void Awake()
        {
            textFormat = new DW.TextFormat(Global.dwFactory, "Segoe UI", 24f);
            textBrush = new D2D1.SolidColorBrush(Global.deviceContext2D, new RawColor4(1, 1, 1, 1));
            backgroundBrush = new D2D1.SolidColorBrush(Global.deviceContext2D, new RawColor4(0, 0, 0, 0.4f));
        }

        private void DrawText(D2D1.DeviceContext context, string text, Vector2 origin, RawRectangleF bgPadding, bool right = false)
        {
            context.TextAntialiasMode = D2D1.TextAntialiasMode.Grayscale;
            string[] lines = text.TrimStart().TrimEnd().Split(new char[] { '\n' }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string tmp = line.TrimStart().TrimEnd();
                if (string.IsNullOrWhiteSpace(tmp))
                {
                    origin.Y += textFormat.FontSize;
                    continue;
                }
                RawVector2 orig = origin;
                DW.TextLayout layout = new DW.TextLayout(Global.dwFactory, tmp, textFormat, Global.targetBitmap2D.Size.Width - orig.X, textFormat.FontSize);
                float height = layout.Metrics.Height;
                layout.MaxWidth = layout.Metrics.WidthIncludingTrailingWhitespace;
                layout.MaxHeight = height;
                if (right)
                {
                    orig.X = Global.targetBitmap2D.Size.Width - orig.X - layout.MaxWidth;
                }
                context.FillRectangle(new RawRectangleF(orig.X - bgPadding.Left, orig.Y - bgPadding.Top, orig.X + layout.MaxWidth + bgPadding.Right, orig.Y + height + bgPadding.Right), backgroundBrush);
                context.DrawTextLayout(orig, layout, textBrush, D2D1.DrawTextOptions.NoSnap | D2D1.DrawTextOptions.DisableColorBitmapSnapping | D2D1.DrawTextOptions.EnableColorFont);
                layout.Dispose();
                origin.Y += height;
            }
        }

        public override void Update()
        {
            if (Input.GetKeyDown(WIN.Keys.F3)) f3Down = true;
            else if (Input.GetKeyUp(WIN.Keys.F3))
            {
                if (f3Down && World.Instance.WorldLoaded)
                    showDebug = !showDebug;
                f3Down = false;
            }

            if (f3Down && Input.GetKeyDown(WIN.Keys.T) && World.Instance.WorldLoaded)
            {
                f3Down = false;
                World.Instance.ReloadResources(false);
            }

            if (f3Down && Input.GetKeyDown(WIN.Keys.A) && World.Instance.WorldLoaded)
            {
                f3Down = false;
                World.Instance.ReloadChunks();
            }

            if (Input.GetKey(WIN.Keys.C) && f3CDownTime < 10f)
            {
                f3Down = false;
                f3CDownTime += Time.deltaTime;
                int tl = 10 - Mathf.FloorToInt(f3CDownTime);
                if (prevF3CDownTime != tl)
                {
                    prevF3CDownTime = tl;
                    log("Crashing in " + tl);
                }
                if (f3CDownTime >= 10f)
                {
                    App.ForceCrash(new StackOverflowException("Debug Crash"));
                }
            }
            else f3CDownTime = 0;

            if (!showDebug) return;

            Global.CurrentProcess.Refresh();
            frameCount++;
            fpsCounter += Time.deltaTime;
            if (fpsCounter > 1.0f / fpsUpdateRate)
            {
                fps = frameCount / fpsCounter;
                frameCount = 0;
                fpsCounter -= 1.0f / fpsUpdateRate;
            }

            chunkPos.X = Mathf.FloorToInt(World.Instance.player.transform.position.X / (float)StaticData.ChunkWidth);
            chunkPos.Y = Mathf.FloorToInt(World.Instance.player.transform.position.Y / (float)StaticData.ChunkWidth);
            chunkPos.Z = Mathf.FloorToInt(World.Instance.player.transform.position.Z / (float)StaticData.ChunkWidth);
            playerChunkPos.X = Mathf.FloorToInt(World.Instance.player.transform.position.X) - chunkPos.X * StaticData.ChunkWidth;
            playerChunkPos.Y = Mathf.FloorToInt(World.Instance.player.transform.position.Y) - chunkPos.Y * StaticData.ChunkWidth;
            playerChunkPos.Z = Mathf.FloorToInt(World.Instance.player.transform.position.Z) - chunkPos.Z * StaticData.ChunkWidth;

            Camera.main.transform.rotation.ToEuler(out rotation);
        }

        public override void Render2D(D2D1.DeviceContext context)
        {
            if (!showDebug) return;

            DrawText(context, $@"
Easy Craft 0.2.0-apha (0.2.0/vanilla/alpha)
{fps} fps T: 60 B: 0
Integrated server @ 0 ms ticks, 0 tx, 0 rx
C: {World.Instance.ActiveChunkCount}/{World.Instance.LoadedChunkCount} D: {World.Instance.ViewDistance}, pC: 000, pU: 00, aB: 0
E: 0/0, B: 0
P: 0. T: 0
TD: {(App.IsThreadDone ? "1" : "0")}, TL: {(App.IsThreadLocked ? "1" : "0")}
Client Chunk Cache: -1, -1
ServerChunkCache: -1
{World.Instance.GetCurrentBiome().FullID} FC: 0

XYZ: {Camera.main.transform.position.X} / {Camera.main.transform.position.Y} / {Camera.main.transform.position.Z}
Block: {Mathf.FloorToInt(World.Instance.player.transform.position.X)} {Mathf.FloorToInt(World.Instance.player.transform.position.Y)} {Mathf.FloorToInt(World.Instance.player.transform.position.Z)}
Chunk: {playerChunkPos.X} {playerChunkPos.Y} {playerChunkPos.Z} in {chunkPos.X} {chunkPos.Y} {chunkPos.Z}
Facing: {rotation.X} / {rotation.Y}
CH S: {World.Instance.HighestNonAir} M: {World.Instance.HighestNonAir}
", new Vector2(0, 0), new RawRectangleF(0, 0, 0, 0));

            DrawText(context, $@"
.NET: {Environment.Version} {(Environment.Is64BitProcess ? "64" : "32")}bit
Mem: {Global.CurrentProcess.WorkingSet64}B
{(App.ConsoleStarted ? $"Console: {Program.console.LogQueueCount}/{Program.console.LogBatchSize}{(Program.console.LogPipeOverflow ? " OVF" : string.Empty)}{(Program.console.LogBatch ? ":BATCH" : string.Empty)}" : string.Empty)}
", new Vector2(0, 0), new RawRectangleF(0, 0, 0, 0), true);
        }

        public override void OnDestroy()
        {
            dispose(textBrush);
            dispose(backgroundBrush);
            dispose(textFormat);
        }
    }
}