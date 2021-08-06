using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public static class BuildManager
    {
        public static List<Scene> Scenes;
        public static Dictionary<string, int> SceneNames;

        static BuildManager()
        {
            Scenes = new List<Scene>();
            SceneNames = new Dictionary<string, int>();
        }

        public static void AddScene(Scene scene)
        {
            SceneNames.Add(scene.name, Scenes.Count);
            Scenes.Add(scene);
        }
    }
}
