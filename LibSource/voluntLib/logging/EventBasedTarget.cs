using System;
using NLog;
using NLog.Targets;

namespace voluntLib.logging
{
    [Target("EventBasedTarget")]
    public sealed class EventBasedTarget : TargetWithLayout
    {
        public event EventHandler<LogEventInfoArg> ApplicationLog;
        
        protected override void Write(LogEventInfo logEvent)
        {
            var eventArg = new LogEventInfoArg
            {
                Level = logEvent.Level,
                Message = logEvent.FormattedMessage,
                Location = logEvent.LoggerName,
                Timestamp = logEvent.TimeStamp
            };

            OnApplicationLog(eventArg);
        }

        private void OnApplicationLog(LogEventInfoArg e)
        {
            var handler = ApplicationLog;
            if (handler != null) handler(this, e);
        }
    }
}