using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup;

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
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        }

        private static void HandleException_DisplayError(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (unhandledExceptionEventArgs.ExceptionObject is XamlParseException e)
            {
                ShowInfoMessageBox(e.InnerException?.ToString() ?? "error displaying error");
            }
            else ShowInfoMessageBox(unhandledExceptionEventArgs.ExceptionObject.ToString());
        }
    }
}