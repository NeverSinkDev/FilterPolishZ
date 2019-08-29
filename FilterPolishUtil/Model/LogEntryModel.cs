using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Model
{
    public class LogEntryModel
    {
        public DateTime Time { get; set; }
        public LoggingLevel Level { get; set; }
        public string Source { get; set; }
        public string Information { get; set; }
        public string MethodName { get; set; }
    }

    public enum LoggingLevel
    {
        Debug,
        Info,
        Warning,
        Errors,
        Item
    }
}
