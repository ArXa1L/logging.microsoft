using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Values;
using Vostok.Logging.Microsoft.Tests.Helpers;
using LogLevel = Vostok.Logging.Abstractions.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using VostokLogEvent = Vostok.Logging.Abstractions.LogEvent;

namespace Vostok.Logging.Microsoft.Tests
{
    [TestFixture]
    public class MicrosoftLog_BasedOnVostok_Tests
    {
        private MemoryLog oracleLog;
        private ILogger microsoftLogger;
        private MicrosoftLog microsoftLog;

        [SetUp]
        public void SetUp()
        {
            oracleLog = new MemoryLog();
            microsoftLogger = new VostokLoggerProvider.Logger(oracleLog, Array.Empty<string>());
            microsoftLog = new MicrosoftLog(microsoftLogger);
        }

        [TestCase(MsLogLevel.Warning, LogLevel.Error, true)]
        [TestCase(MsLogLevel.Warning, LogLevel.Warn, true)]
        [TestCase(MsLogLevel.Warning, LogLevel.Info, false)]
        public void IsEnabledFor_ReturnsValidResult(MsLogLevel minLevel, LogLevel level, bool expectedEnabled)
        {
            var provider = new VostokLoggerProvider(oracleLog);
            var loggerFilterOptions = new LoggerFilterOptions {MinLevel = minLevel};
            var loggerFactory = new LoggerFactory(new[] {provider}, loggerFilterOptions);

            var microsoftLogger1 = loggerFactory.CreateLogger<MicrosoftLog_BasedOnPureLogger_Tests>();

            var microsoftLog1 = new MicrosoftLog(microsoftLogger1);
            microsoftLog1.IsEnabledFor(level).Should().Be(expectedEnabled);
        }

        [Test]
        public void Log_SimpleMessage_LogsValidEvent()
        {
            var expectedLogEvent = new VostokLogEvent(LogLevel.Debug, DateTimeOffset.Now, "message");

            microsoftLog.Log(expectedLogEvent);

            oracleLog.Events.Single().Should().BeSameAs(expectedLogEvent);
        }

        [Test]
        public void Log_MessageWithNamedPlaceholders_LogsValidEvent()
        {
            var expectedLogEvent = new VostokLogEvent(LogLevel.Debug, DateTimeOffset.Now, "message {p1} {p2}")
                .WithProperty("p1", "v1")
                .WithProperty("p2", "v2");

            microsoftLog.Log(expectedLogEvent);

            oracleLog.Events.Single().Should().BeSameAs(expectedLogEvent);
        }

        [Test]
        public void Log_MessageWithPositionPlaceholders_LogsValidEvent()
        {
            var expectedLogEvent = new VostokLogEvent(LogLevel.Fatal, DateTimeOffset.Now, "message {0} {1}")
                .WithProperty("0", "v1")
                .WithProperty("1", "v2");

            microsoftLog.Log(expectedLogEvent);

            oracleLog.Events.Single().Should().BeSameAs(expectedLogEvent);
        }

        [Test]
        public void Log_WithException_LogsValidEvent()
        {
            var exception = new Exception("exception");
            var expectedLogEvent = new VostokLogEvent(LogLevel.Info, DateTimeOffset.Now, "message", exception);

            microsoftLog.Log(expectedLogEvent);

            oracleLog.Events.Single().Should().BeSameAs(expectedLogEvent);
        }

        [Test]
        public void Log_WithSimpleMicrosoftLoggerScope_LogsValidEvent()
        {
            const string scope = "s1";
            var expectedLogEvent = new VostokLogEvent(LogLevel.Info, DateTimeOffset.Now, "message")
                .WithProperty("operationContext", new OperationContextValue(scope));

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info("message");

                oracleLog.Events.Single()
                    .Should()
                    .BeEquivalentTo(expectedLogEvent, o => o.Excluding(x => x.Timestamp));
            }
        }

        [Test]
        public void Log_WithSimpleMicrosoftLoggerScopeAndException_LogsValidEvent()
        {
            const string scope = "s1";
            var exception = new Exception("exception");
            var expectedLogEvent = new VostokLogEvent(LogLevel.Info, DateTimeOffset.Now, "message", exception)
                .WithProperty("operationContext", new OperationContextValue(scope));

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info(exception, "message");

                oracleLog.Events.Single()
                    .Should()
                    .BeEquivalentTo(expectedLogEvent, o => o.Excluding(x => x.Timestamp));
            }
        }

        [Test]
        public void Log_WithComplexMicrosoftLoggerScope_LogsValidEvent()
        {
            var expectedLogEvent = new VostokLogEvent(LogLevel.Info, DateTimeOffset.Now, "message")
                .WithProperty("operationContext", new OperationContextValue("{ id = 1, name = 2 }"));

            using (microsoftLogger.BeginScope(new {id = 1, name = 2}))
            {
                microsoftLog.Info("message");

                oracleLog.Events.Single()
                    .Should()
                    .BeEquivalentTo(expectedLogEvent, o => o.Excluding(x => x.Timestamp));
            }
        }

        [Test]
        public void Log_WithComplexMicrosoftLoggerScopeAndException_LogsValidEvent()
        {
            var exception = new Exception("exception");

            var expectedLogEvent = new VostokLogEvent(LogLevel.Info, DateTimeOffset.Now, "message", exception)
                .WithProperty("operationContext", new OperationContextValue("{ id = 1, name = 2 }"));

            using (microsoftLogger.BeginScope(new {id = 1, name = 2}))
            {
                microsoftLog.Info(exception, "message");

                oracleLog.Events.Single()
                    .Should()
                    .BeEquivalentTo(expectedLogEvent);
            }
        }
    }
}