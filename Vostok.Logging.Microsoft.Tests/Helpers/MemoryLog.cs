using System.Collections.Generic;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;
using VostokLogLevel = Vostok.Logging.Abstractions.LogLevel;

namespace Vostok.Logging.Microsoft.Tests.Helpers
{
    internal class MemoryLog : ILog
    {
        public readonly List<LogEvent> Events = new List<LogEvent>();

        public void Log(LogEvent @event)
        {
            lock (Events)
                Events.Add(@event);
        }

        public bool IsEnabledFor(VostokLogLevel level) => true;

        public ILog ForContext(string context)
        {
            return new SourceContextWrapper(this, context);
        }
    }
}