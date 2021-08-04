using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleWrapper
{
    public static class ExceptionHandler
    {
        /// <summary>
        /// Adds exception handling into Windows Forms app
        /// </summary>
        /// <param name="appDomain">The app domain to catch errors</param>
        /// <param name="handler">Custom logic for error handling</param>
        public static void CatchExceptions(AppDomain appDomain, Action<object, bool, Exception> handler)
        {
            appDomain.UnhandledException += (s, e) => { handler(s, e.IsTerminating, e.ExceptionObject as Exception); };
            Application.ThreadException += (s, e) => { handler(s, false, e.Exception); };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }
    }
}
