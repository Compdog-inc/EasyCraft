using System;
using System.Windows.Forms;
using EasyCraft.engine;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Pipes;
using System.Collections.Generic;
using ConsoleWrapper;

namespace EasyCraft
{
    static class Program
    {
        public static ConsoleWrapper.Console console;

        [STAThread]
        static int Main(string[] args)
        {
            ExceptionHandler.CatchExceptions(AppDomain.CurrentDomain, (sender, terminating, e) =>
            {
                Environment.ExitCode = App.ExitCode = e.HResult;
                Debug.LogError(e.Message.TrimEnd() + "\n" + e.StackTrace.TrimEnd() + (terminating ? "\nThe application will now terminate." : ""), sender);

                string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"TerrainGenTest\crash\");
                if (!Directory.Exists(logFile)) Directory.CreateDirectory(logFile);
                logFile += "crash_" + DateTime.Now.ToString("MM-dd-yyyy-H-mm-ss") + ".log";
                using (StreamWriter writer = new StreamWriter(logFile))
                    Debug.WriteLog(writer);

                if (MessageBox.Show(Global.window, "Error: " + e.Message.TrimEnd() + (terminating ? "\nThe application will now terminate.\nDo you want to send a crash report?" : ""), "AppDomain Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("CrashHandler.exe", $"/C \"{logFile}\"");
                }
            });

            console = ConsoleWrapper.Console.Default;
            console.EnableBatching = false;
            console.ConsoleClosed += (s, e) => System.Console.WriteLine("Console cloed with exit code " + e.ExitCode);
            console.PipeOverflow += (s, e) => System.Console.WriteLine("Pipe overflow! Batching calls");

            switch(console.Create(out Exception ex)){
                case ConsoleResult.CREATE_ERROR:
                    System.Console.WriteLine("Error creating process: " + ex);
                    break;
                case ConsoleResult.PIPE_ERROR:
                    System.Console.WriteLine("Pipe error: " + ex);
                    break;
                case ConsoleResult.NOT_STARTED:
                    System.Console.WriteLine("Console not started");
                    break;
            }

            using (Game game = new Game())
            {
                game.Run();
            }

            console.Dispose();

            return App.ExitCode;
        }
    }
}
