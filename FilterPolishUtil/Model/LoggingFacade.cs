using FilterPolishUtil.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Model
{
    public class LoggingFacade : ICleanable
    {
        private static LoggingFacade instance;

        public ObservableCollection<LogEntryModel> Logs { get; set; } = new ObservableCollection<LogEntryModel>();

        public Action<string> CustomHighImportanceLoggingMessage { get; set; } = ((s) => { });
        public Action<string> CustomLoggingMessage { get; set; } = ((s) => { });

        private LoggingFacade()
        {

        }

        public object _itemsLock;

        public void SetLock(object localLock)
        {
            _itemsLock = localLock;
        }

        public static LoggingFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new LoggingFacade();
                instance._itemsLock = new object();
            }

            return instance;
        }

        public static void LogWarning(string info)
        {
            GetInstance().CustomHighImportanceLoggingMessage("[WARNING] " + info);
            GetInstance().CustomLoggingMessage("[WARNING] " + info);
            GetInstance().Log(info, LoggingLevel.Warning);
        }

        public static void LogError(string info)
        {
            GetInstance().CustomHighImportanceLoggingMessage("[ERROR] " + info);
            GetInstance().CustomLoggingMessage("[ERROR] " + info);
            GetInstance().Log(info, LoggingLevel.Errors);
        }

        public static void LogItem(string info)
        {
            GetInstance().Log(info, LoggingLevel.Item);
        }

        public static void LogInfo(string info, bool showMessage = false)
        {
            if (showMessage)
            {
                GetInstance().CustomHighImportanceLoggingMessage(info);
            }

            GetInstance().CustomLoggingMessage(info);
            GetInstance().Log(info, LoggingLevel.Info);
        }

        public static void LogDebug(string info)
        {
            GetInstance().CustomLoggingMessage(info);
            GetInstance().Log(info, LoggingLevel.Debug);
        }

        public void SetCustomLoggingMessage(Action<string> action)
        {
            this.CustomHighImportanceLoggingMessage = action;
        }

        public void SetCustomHighImportanceLoggingMessage(Action<string> action)
        {
            this.CustomHighImportanceLoggingMessage = action;
        }

        public void Log(string info, LoggingLevel level)
        {
            var mth = new StackTrace().GetFrame(2).GetMethod();
            var cls = mth?.ReflectedType?.Name ?? "unknown name";
            var methodName = mth.Name;

            lock(_itemsLock)
            {
                this.Logs.Add(new LogEntryModel()
                {
                    Level = level,
                    Time = DateTime.Now,
                    Information = info,
                    Source = cls,
                    MethodName = methodName
                });
            }
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }

        public void Clean()
        {
            this.ClearLogs();
            instance = null;
        }
    }
}
