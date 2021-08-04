using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyCraft.CrashHandler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Dictionary<string, string> argDict = new Dictionary<string, string>();
                string currentKey = null;

                foreach (string arg in args)
                {
                    if (currentKey == null)
                        currentKey = arg.ToUpper();
                    else
                    {
                        argDict.Add(currentKey, arg);
                        currentKey = null;
                    }

                }

                if (argDict.ContainsKey("/C"))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new CrashReport(argDict));
                }
            }
        }
    }
}
