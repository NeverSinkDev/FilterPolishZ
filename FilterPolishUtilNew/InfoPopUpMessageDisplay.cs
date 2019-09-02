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
            MessageBox.Show(messageText, "I: " + messageText);
        }
        
        public static void ShowError(string messageText)
        {
            MessageBox.Show(messageText, "E: " + messageText);
        }

        public static bool DisplayQuestionMessageBox(string questionText)
        {
            var res = MessageBox.Show(questionText, "Q: " + questionText, MessageBoxButtons.YesNo);

            switch (res)
            {
                case DialogResult.Yes:
                case DialogResult.OK:
                    return true;
                
                default:
                    return false;
            }
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
                ShowError(e.InnerException?.ToString() ?? "error displaying error");
            }
            else ShowError(unhandledExceptionEventArgs.ExceptionObject.ToString());
        }
    }
}