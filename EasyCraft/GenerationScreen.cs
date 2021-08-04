using System;
using D2D1 = SharpDX.Direct2D1;
using EasyCraft.engine;
using DW = SharpDX.DirectWrite;
using SharpDX;
using SharpDX.Mathematics.Interop;
using WIN = System.Windows.Forms;
using EasyCraft.engine.extensions;
using System.Collections.Generic;
using SharpDX.WIC;

namespace EasyCraft
{
    public class GenerationScreen : Behavior
    {
        private struct TextLine
        {
            public string text;
            public DW.TextLayout textLayout;
            public Vector2 position;

            public TextLine(string _text, DW.TextLayout layout, float xPos, float yPos)
            {
                text = _text;
                textLayout = layout;
                position = new Vector2(xPos, yPos);
            }
        }

        private DW.TextFormat textFormat;
        private D2D1.Brush textBrush;
        private string text;

        private Vector2 screenSize;
        private Vector2 prevScreenSize;
        private float textOffset;
        private List<TextLine> textLines = new List<TextLine>();

        private D2D1.Bitmap backgroundImage;
        private D2D1.Effect transformEffect;
        private D2D1.Effect tileEffect;
        private float backgroundTileSize = 128f;

        public override void Awake()
        {
            textFormat = new DW.TextFormat(Global.dwFactory, "Segoe UI", 32f);
            textBrush = new D2D1.SolidColorBrush(Global.deviceContext2D, new RawColor4(1, 1, 1, 1));

            BitmapSource bgImg = TextureLoader.LoadBitmap(Global.imagingFactory, "easycraft/textures/blocks/dirt.png");
            backgroundImage = D2D1.Bitmap.FromWicBitmap(Global.deviceContext2D, bgImg);
            bgImg.Dispose();
            
            transformEffect = new D2D1.Effect(Global.deviceContext2D, D2D1.Effect.AffineTransform2D);
            transformEffect.Cached = true;
            transformEffect.SetInput(0, backgroundImage, true);

            transformEffect.SetEnumValue((int)D2D1.AffineTransform2DProperties.InterpolationMode, D2D1.AffineTransform2DInterpolationMode.NearestNeighbor);
            transformEffect.SetValue((int)D2D1.AffineTransform2DProperties.TransformMatrix, Matrix3x2.Scaling(backgroundTileSize / 16f));

            tileEffect = new D2D1.Effect(Global.deviceContext2D, D2D1.Effect.Tile);
            tileEffect.Cached = true;
            tileEffect.SetInputEffect(0, transformEffect);
            tileEffect.SetValue((int)D2D1.TileProperties.Rectangle, new RawRectangleF(0, 0, backgroundTileSize, backgroundTileSize));
        }

        public override void Start()
        {
            screenSize = new Vector2(Global.targetBitmap2D.Size.Width, Global.targetBitmap2D.Size.Height);
            prevScreenSize = screenSize;
        }

        private void CalculateSize()
        {
            string[] lines = text.TrimStart().TrimEnd().Split(new char[] { '\n' }, StringSplitOptions.None);
            float totalHeight = 0;
            int i = 0;
            foreach (string line in lines)
            {
                if (i >= textLines.Count)
                {
                    // New lines added
                    DW.TextLayout layout = new DW.TextLayout(Global.dwFactory, line, textFormat, 0, 0);
                    layout.WordWrapping = DW.WordWrapping.NoWrap;
                    textLines.Add(new TextLine(line, layout,
                        screenSize.X / 2f - layout.Metrics.WidthIncludingTrailingWhitespace / 2f,
                        totalHeight));
                    totalHeight += layout.Metrics.Height;
                }
                else
                {
                    if (line != textLines[i].text)
                    {
                        // Text edited
                        DW.TextLayout layout = new DW.TextLayout(Global.dwFactory, line, textFormat, 0, 0);
                        layout.WordWrapping = DW.WordWrapping.NoWrap;
                        totalHeight += layout.Metrics.Height;
                        textLines[i].textLayout.Dispose();
                        textLines[i] = new TextLine(line, layout,
                            screenSize.X / 2f - layout.Metrics.WidthIncludingTrailingWhitespace / 2f, 
                            totalHeight);
                        totalHeight += layout.Metrics.Height;
                    }
                    else
                    {
                        if(screenSize != prevScreenSize)
                        {
                            // Screen resized
                            textLines[i] = new TextLine(
                                textLines[i].text, textLines[i].textLayout, 
                                screenSize.X / 2f - textLines[i].textLayout.Metrics.WidthIncludingTrailingWhitespace / 2f,
                                textLines[i].position.Y);
                        }
                        totalHeight += textLines[i].textLayout.Metrics.Height;
                    }
                }

                i++;
            }

            // Dispose and remove deleted lines
            if (i < textLines.Count)
            {
                for (int j = i; j < textLines.Count; j++)
                {
                    textLines[i].textLayout.Dispose();
                }
                textLines.RemoveRange(i, textLines.Count - i);
            }

            // Calculate global height offset
            textOffset = screenSize.Y / 2f - totalHeight / 2f;
        }

        private void DrawText(D2D1.DeviceContext context, Vector2 offset)
        {
            context.TextAntialiasMode = D2D1.TextAntialiasMode.Grayscale;
            foreach (TextLine line in textLines)
            {
                context.DrawTextLayout(line.position + offset + new Vector2(0, textOffset), line.textLayout, textBrush, 
                    D2D1.DrawTextOptions.NoSnap |
                    D2D1.DrawTextOptions.DisableColorBitmapSnapping | 
                    D2D1.DrawTextOptions.EnableColorFont
                    );
            }
        }

        public override void Update()
        {
            prevScreenSize = screenSize;
            screenSize = new Vector2(Global.targetBitmap2D.Size.Width, Global.targetBitmap2D.Size.Height);

            text = string.Format("Generating World\n{0} / {1} chunks generated.", World.Instance.ActiveChunkCount, Math.Pow(World.Instance.ViewDistance * 2, 2));
            CalculateSize();
        }

        public override void Render2D(D2D1.DeviceContext context)
        {
            context.DrawImage(tileEffect, D2D1.InterpolationMode.NearestNeighbor);
            DrawText(context, new Vector2(0, 0));
        }

        public override void OnDestroy()
        {
            dispose(backgroundImage);
            dispose(textBrush);
            dispose(textFormat);
            dispose(tileEffect);
            dispose(transformEffect);
        }
    }
}
