using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging
{
    internal static class Log
    {


        public static Logger Content { get; private set; }

        static Log()
        {
            LogManager.ReconfigExistingLoggers();
            Content = LogManager.GetCurrentClassLogger();
        }
    }
}
