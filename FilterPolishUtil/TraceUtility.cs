using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishUtil
{
    public static class TraceUtility
    {
        public static void Check(bool condition, string message)
        {
            if (condition)
            {
                LoggingFacade.LogError(message);
                Throw(message);
            }
        }

        public static void Throw(string message)
        {
            LoggingFacade.LogError(message);
            throw new Exception(message);
        }
    }
}
