using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace OpenKeyboard
{
    public partial class App : Application
    {
        public void App_Startup(object sender, StartupEventArgs e)
        {
            Process[] allProcess = Process.GetProcesses();
            int n = allProcess.Where(p => p.ProcessName.Equals(Process.GetCurrentProcess().ProcessName)).Count();
            if (n > 1)
            {
                Application.Current.Shutdown();
                return;
            }
            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }//event

        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.InnerException != null) vLogger.Exception("app.UnhandledException", e.Exception.InnerException);
            else vLogger.Exception("app.UnhandledException", e.Exception);

            e.Handled = true;
            Application.Current.Shutdown();
        }//func
    }//cls
}//ns
