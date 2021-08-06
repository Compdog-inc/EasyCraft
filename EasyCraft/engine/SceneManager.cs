using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public static class SceneManager
    {
        public static Scene LoadedScene { get => loadedScene; }

        private static Scene loadedScene = null;

        public static Task<bool> LoadSceneAsync(string name)
        {
            return new Task<bool>(() => LoadScene(name));
        }

        public static Task<bool> LoadSceneAsync(int id)
        {
            return new Task<bool>(() => LoadScene(id));
        }

        public static bool LoadScene(string name)
        {
            if (BuildManager.SceneNames.ContainsKey(name))
            {
                return LoadScene(BuildManager.SceneNames[name]);
            } else
            {
                Debug.LogError($"Tried loading non existing scene '{name}'");
                return false;
            }
        }

        public static bool LoadScene(int id)
        {
            if(id >= BuildManager.Scenes.Count)
            {
                Debug.LogError($"Tried loading non existing scene '{id}'");
                return false;
            }

            if (loadedScene != null)
            {
                loadedScene.InitUnLoad();
                try
                {
                    loadedScene.DestroySceneObjects();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error destroying scene objects. [{e.HResult}]" + e.Message.TrimEnd() + "\n" + e.StackTrace.TrimEnd());
                    loadedScene.FailUnLoad(e);
                    return false;
                }
                Debug.Log($"Unloaded scene '{loadedScene.name}'");
                loadedScene = null;
            }

            BuildManager.Scenes[id].InitLoad();
            try
            {
                BuildManager.Scenes[id].CreateSceneObjects();
            } catch(Exception e)
            {
                Debug.LogError($"Error creating scene objects. [{e.HResult}]" + e.Message.TrimEnd() + "\n" + e.StackTrace.TrimEnd());
                BuildManager.Scenes[id].FailLoad(e);
                return false;
            }
            loadedScene = BuildManager.Scenes[id];
            BuildManager.Scenes[id].EndLoad();
            Debug.Log($"Loaded scene '{loadedScene.name}'");
            return true;
        }

        public static bool UnloadScene()
        {
            if (loadedScene != null)
            {
                loadedScene.InitUnLoad();
                try
                {
                    loadedScene.DestroySceneObjects();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error destroying scene objects. [{e.HResult}]" + e.Message.TrimEnd() + "\n" + e.StackTrace.TrimEnd());
                    loadedScene.FailUnLoad(e);
                    return false;
                }
                Debug.Log($"Unloaded scene '{loadedScene.name}'");
                loadedScene = null;
                return true;
            } else
            {
                Debug.LogWarning("No loaded scenes to unload");
                return false;
            }
        }
    }
}
