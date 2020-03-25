using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;
using Vostok.Logging.Formatting;
using VostokLogLevel = Vostok.Logging.Abstractions.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Vostok.Logging.Microsoft
{
    [PublicAPI]
    public sealed class MicrosoftLog : ILog
    {
        private const int NullEventId = 0;
        private static readonly OutputTemplate Template = OutputTemplate.Parse(
            $"{{{WellKnownProperties.TraceContext}:w}}" +
            $"{{{WellKnownProperties.OperationContext}:w}}" +
            $"{{{WellKnownProperties.SourceContext}:w}}" +
            $"{{{WellKnownTokens.Message}}}");

        private readonly ILogger logger;
        private readonly MicrosoftLogSettings settings;

        public MicrosoftLog([NotNull] ILogger logger, [CanBeNull] MicrosoftLogSettings settings = null)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? new MicrosoftLogSettings();
        }

        public void Log(LogEvent @event)
        {
            if (@event is null)
                return;

            var logLevel = TranslateLogLevel(@event.Level);
            if (!IsEnabledFor(logLevel))
                return;

            logger.Log(logLevel, NullEventId, new VostokLogEventWrapper(@event), @event.Exception, FormatMessage);

            string FormatMessage(VostokLogEventWrapper logEventWrapper, Exception ex)
            {
                return settings.UseVostokTemplate
                    ? LogEventFormatter.Format(logEventWrapper.LogEvent, Template)
                    : LogMessageFormatter.Format(logEventWrapper.LogEvent);
            }
        }

        public bool IsEnabledFor(VostokLogLevel level)
        {
            var logLevel = TranslateLogLevel(level);
            return IsEnabledFor(logLevel);
        }

        private bool IsEnabledFor(MicrosoftLogLevel logLevel)
        {
            return logger.IsEnabled(logLevel);
        }

        public ILog ForContext(string context)
        {
            return new SourceContextWrapper(this, context);
        }

        private static MicrosoftLogLevel TranslateLogLevel(VostokLogLevel logLevel)
        {
            switch (logLevel)
            {
                case VostokLogLevel.Debug:
                    return MicrosoftLogLevel.Debug;
                case VostokLogLevel.Info:
                    return MicrosoftLogLevel.Information;
                case VostokLogLevel.Warn:
                    return MicrosoftLogLevel.Warning;
                case VostokLogLevel.Error:
                    return MicrosoftLogLevel.Error;
                case VostokLogLevel.Fatal:
                    return MicrosoftLogLevel.Critical;
                default:
                    return MicrosoftLogLevel.None;
            }
        }
    }
}