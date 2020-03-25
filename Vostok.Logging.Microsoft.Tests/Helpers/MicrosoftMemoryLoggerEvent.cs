using System;
using Microsoft.Extensions.Logging;

namespace Vostok.Logging.Microsoft.Tests.Helpers
{
    internal class MicrosoftMemoryLoggerEvent
    {
        public LogLevel LogLevel { get; set; }

        public object State { get; set; }

        public Exception Exception { get; set; }

        public object Scope { get; set; }

        public string Message { get; set; }
        
        public string Category { get; set; }
    }
}