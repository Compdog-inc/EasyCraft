using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraft.engine
{
    public static class Debug
    {
        private static List<string> history = new List<string>();

        public static void Log(object obj, object sender = null)
        {
            string s = string.Format("[LOG {0} {1}]: {2}", DateTime.Now.ToString(), sender != null ? sender.GetType().FullName : "UNKNOWN", obj.ToString());
            history.Add(s);
#if DEBUG
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);
            Console.ForegroundColor = prev;
#endif
            Program.console.Log(s);
        }

        public static void LogWarning(object obj, object sender = null)
        {
            string s = string.Format("[WARNING {0} {1}]: {2}", DateTime.Now.ToString(), sender != null ? sender.GetType().FullName : "UNKNOWN", obj.ToString());
            history.Add(s);
#if DEBUG
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = prev;
#endif
            Program.console.LogWarning(s);
        }

        public static void LogError(object obj, object sender = null)
        {
            string s = string.Format("[ERROR {0} {1}]: {2}", DateTime.Now.ToString(), sender != null ? sender.GetType().FullName : "UNKNOWN", obj.ToString());
            history.Add(s);
#if DEBUG
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ForegroundColor = prev;
#endif
            Program.console.LogError(s);
        }

        public static void WriteLog(StreamWriter writer, bool clear = false)
        {
            foreach (string h in history) writer.WriteLine(h);
            if (clear) history.Clear();
        }
    }
}
