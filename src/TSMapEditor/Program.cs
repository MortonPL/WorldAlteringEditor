using Rampastring.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
#if WINFORMS
using System.Windows.Forms;
#endif
using TSMapEditor.Rendering;

namespace TSMapEditor
{
    static class Program
    {
        public static string[] args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Program.args = args;
#if WINFORMS
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.ThreadException += Application_ThreadException;
#endif
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            DirectoryInfo gameDirectory = SafePath.GetDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);
            Environment.CurrentDirectory = gameDirectory.FullName;

            new GameClass().Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void HandleException(Exception ex)
        {
            string message = "The map editor failed to launch.\r\n\r\nReason: " + ex.Message + "\r\n\r\n Stack trace: " + ex.StackTrace;
#if WINFORMS
            MessageBox.Show(message);
#else
            Logger.Log(message);
            Console.WriteLine(message);
            Process.Start(new ProcessStartInfo {
                FileName = Environment.CurrentDirectory + "/MapEditorLog.log",
                UseShellExecute = true
            });
#endif
        }

        public static void DisableExceptionHandler()
        {
#if WINFORMS
            Application.ThreadException -= Application_ThreadException;
#endif
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
    }
}
