using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Collections;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Values;
using Vostok.Logging.Microsoft.Helpers;
using Vostok.Logging.Microsoft.Tests.Helpers;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using VostokLogEvent = Vostok.Logging.Abstractions.LogEvent;

namespace Vostok.Logging.Microsoft.Tests
{
    [TestFixture]
    public class MicrosoftLog_BasedOnPureLogger_Tests
    {
        private const string TestCategoryName = "Vostok.Logging.Microsoft.Tests";

        private MicrosoftLog microsoftLog;
        private MicrosoftMemoryLogger microsoftLogger;

        [SetUp]
        public void SetUp()
        {
            microsoftLogger = new MicrosoftMemoryLogger(TestCategoryName);
            microsoftLog = new MicrosoftLog(microsoftLogger);
        }

        [Test]
        public void Log_SimpleMessage_LogsValidEvent()
        {
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Warning,
                Message = "message",
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.Warn("message");

            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }

        [Test]
        public void Log_MessageWithNamedPlaceholders_LogsValidEvent()
        {
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Error,
                Message = "message v1 v2",
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message {p1} {p2}",
                        ["p1"] = "v1",
                        ["p2"] = "v2"
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.Error("message {p1} {p2}", "v1", "v2");

            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }

        [Test]
        public void Log_MessageWithPositionPlaceholders_LogsValidEvent()
        {
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Critical,
                Message = "message v1 v2",
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message {0} {1}",
                        ["0"] = "v1",
                        ["1"] = "v2"
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.Fatal("message {0} {1}", "v1", "v2");

            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }

        [Test]
        public void Log_WithException_LogsValidEvent()
        {
            var exception = new Exception("exception");
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Exception = exception,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.Info(exception, "message");

            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }

        [Test]
        public void Log_WithSimpleMicrosoftLoggerScope_LogsValidEvent()
        {
            const string scope = "s1";
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Scope = scope,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info("message");

                microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
            }
        }

        [Test]
        public void Log_WithMultiplyMicrosoftLoggerScopes_LogsValidEvent()
        {
            const string scope1 = "s1";
            const string scope2 = "s2";

            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Scope = scope2,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            using (microsoftLogger.BeginScope(scope1))
            using (microsoftLogger.BeginScope(scope2))
            {
                microsoftLog.Info("message");

                microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
            }
        }

        [Test]
        public void Log_WithSimpleMicrosoftLoggerScopeAndException_LogsValidEvent()
        {
            const string scope = "s1";
            var exception = new Exception("exception");
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Exception = exception,
                Scope = scope,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info(exception, "message");

                microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
            }
        }

        [Test]
        public void Log_WithComplexMicrosoftLoggerScope_LogsValidEvent()
        {
            var scope = new {id = 1, name = 2};

            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Scope = scope,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info("message");

                microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
            }
        }

        [Test]
        public void Log_WithComplexMicrosoftLoggerScopeAndException_LogsValidEvent()
        {
            var scope = new {id = 1, name = 2};
            var exception = new Exception("exception");

            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Information,
                Message = "message",
                Scope = scope,
                Exception = exception,
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message"
                    },
                    StringComparer.Ordinal)
            };

            using (microsoftLogger.BeginScope(scope))
            {
                microsoftLog.Info(new Exception("exception"), "message");

                microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
            }
        }

        [Test]
        public void Log_SimpleMessageWithSimpleContext_LogsValidEvent()
        {
            const string context = "ctx1";
            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Warning,
                Message = "message",
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message",
                        ["sourceContext"] = new SourceContextValue(context)
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.ForContext(context).Warn("message");
            
            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }

        [Test]
        public void Log_SimpleMessageWithMultiplyContexts_LogsValidEvent()
        {
            const string context1 = "ctx1";
            const string context2 = "ctx2";

            var expectedLogEvent = new MicrosoftMemoryLoggerEvent
            {
                Category = TestCategoryName,
                LogLevel = MsLogLevel.Warning,
                Message = "message",
                State = new ImmutableArrayDictionary<string, object>(
                    new Dictionary<string, object>
                    {
                        [MicrosoftLogProperties.OriginalFormatKey] = "message",
                        ["sourceContext"] = new SourceContextValue(new[] {context1, context2})
                    },
                    StringComparer.Ordinal)
            };

            microsoftLog.ForContext(context1).ForContext(context2).Warn("message");

            microsoftLogger.Events.Single().Should().BeEquivalentTo(expectedLogEvent);
        }
    }
}