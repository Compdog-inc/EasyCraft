using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyCraft.engine
{
    public static class App
    {
        [DllImport("kernel32.dll")]
        private static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);

        public static int ExitCode { get; set; } = 0;
        public static bool IsQuitting { get; private set; } = false;
        public static bool WindowResized { get; set; } = false;
        public static bool IsThreadLocked { get; private set; } = false;
        public static bool IsThreadDone { get; internal set; } = false;
        public static bool ConsoleStarted { get => Program.console.IsOpen; }

        public static void Quit()
        {
            IsQuitting = true;
        }

        public static void Quit(int exitCode)
        {
            ExitCode = exitCode;
            Quit();
        }

        public static void CancelQuit()
        {
            IsQuitting = false;
        }

        public static bool SafeDispose(IDisposable d, object sender = null)
        {
            if (d != null)
            {
                d.Dispose();
                return true;
            }

            Debug.LogWarning($"Disposed NULL object", sender);
            return false;
        }

        public static void ThreadLock()
        {
            IsThreadLocked = true;
        }

        public static void ThreadUnlock()
        {
            IsThreadLocked = false;
        }

        public static void WaitForThread()
        {
            while (!IsThreadDone) Thread.Sleep(1);
        }

        public static void ForceCrash(Exception e)
        {
            Thread t = Thread.CurrentThread;
            ThreadPool.QueueUserWorkItem(new WaitCallback(ignored =>
            {
                t.Abort();
                throw e;
            }));
        }
    }
}