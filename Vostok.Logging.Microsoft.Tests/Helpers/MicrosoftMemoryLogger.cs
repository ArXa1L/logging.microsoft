using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Vostok.Logging.Microsoft.Tests.Helpers
{
    internal class MicrosoftMemoryLogger : ILogger
    {
        private readonly string categoryName;

        public MicrosoftMemoryLogger(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public ConcurrentQueue<MicrosoftMemoryLoggerEvent> Events { get; } = new ConcurrentQueue<MicrosoftMemoryLoggerEvent>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return TestDisposable.Instance;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Events.Enqueue(
                new MicrosoftMemoryLoggerEvent
                {
                    Category = categoryName,
                    LogLevel = logLevel,
                    State = state,
                    Exception = exception,
                    Message = formatter(state, exception)
                });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        private class TestDisposable : IDisposable
        {
            public static readonly TestDisposable Instance = new TestDisposable();

            public void Dispose()
            {
            }
        }
    }
}