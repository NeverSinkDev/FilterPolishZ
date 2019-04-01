using System;
using System.Threading;
using System.Windows.Forms;

namespace FilterPolishUtil
{
    public static class InfoPopUpMessageDisplay
    {
        public static void ShowInfoMessageBox(string messageText)
        {
            MessageBox.Show(messageText);
        }

        public static void InitExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleException_DisplayError;
            Application.ThreadException += HandleThreadException_DisplayError;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        }

        private static void HandleException_DisplayError(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            ShowInfoMessageBox("exc");
        }
        
        private static void HandleThreadException_DisplayError(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            ShowInfoMessageBox("exc 2");
        }
    }
}