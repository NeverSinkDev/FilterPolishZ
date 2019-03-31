using System;
using System.Threading;
using System.Windows.Forms;

namespace FilterPolishUtil
{
    public static class InfoPopUpMessageDisplay
    {
        public static void ShowInfoMessageBox(string messageText)
        {
            MessageBox.Show("Filter generation successfully done!");
        }

//        public static void InitExceptionHandling()
//        {
//            AppDomain.CurrentDomain.UnhandledException += HandleException_DisplayError;
//            Application.ThreadException += HandleThreadException_DisplayError;
//        }
//
//        private static void HandleException_DisplayError(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
//        {
//            ShowInfoMessageBox("exc");
//        }
//        
//        private static void HandleThreadException_DisplayError(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
//        {
//            ShowInfoMessageBox("exc 2");
//        }
    }
}