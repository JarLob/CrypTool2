using System;
using NLog;

namespace voluntLib.logging
{
    public class LogEventInfoArg : EventArgs
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string Location { get; set; }
    }
}