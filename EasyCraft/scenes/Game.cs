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

namespace EasyCraft.scenes
{
    public class Game : Scene
    {
        public override string name => "Game";

        public override void CreateSceneObjects()
        {
            genScreen = new GenerationScreen() { name = "Generation Screen" };
            sun = new DirectionalLight() { name = "Sun" };
            sun.transform.position = new Vector3(-1.8f, 1.8f, 1.8f);
            debugScreen = new DebugScreen() { name = "Debug Screen" };
            world = new World() { name = "World", generationScreen = genScreen };
            pauseScreen = new PauseScreen() { name = "Pause Screen", active = false };
            player = new Player() { name = "Player", pauseScreen = pauseScreen };
            camera = new Camera() { name = "Main Camera" };
            camera.transform.SetParent(player.transform);
            world.player = player;
            Global.game.UpdateMainCamera();
        }

        public override void DestroySceneObjects()
        {
            genScreen.Destroy();
            sun.Destroy();
            debugScreen.Destroy();
            world.Destroy();
            pauseScreen.Destroy();
            player.Destroy();
            camera.ForceDispose(true);
            camera.Destroy();
        }

        private Camera camera;
        private Player player;
        private GenerationScreen genScreen;
        private DirectionalLight sun;
        private DebugScreen debugScreen;
        private World world;
        private PauseScreen pauseScreen;
    }
}
