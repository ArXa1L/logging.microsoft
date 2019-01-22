using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Vostok.Logging.Microsoft
{
    /// <inheritdoc />
    /// <summary>
    /// <para>Logger provider for writing log events into Vostok <see cref="ILog"/>.</para>
    /// </summary>
    [PublicAPI]
    public class VostokLoggerProvider : ILoggerProvider
    {
        private readonly ILog log;

        /// <summary>
        /// <para>Create a new <see cref="VostokLoggerProvider"/> for given root <paramref name="log"/></para>
        /// </summary>
        /// <param name="log"><see cref="ILog"/> to write log events to</param>
        public VostokLoggerProvider([NotNull] ILog log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc />
        /// <summary>
        /// <para>Create a new <see cref="ILogger"/> for given <paramref name="categoryName"/></para> 
        /// <para>
        /// If <paramref name="categoryName"/> is null or empty then returned <see cref="ILogger"/> will write events directly into <see cref="log"/>,
        /// otherwise it will write events into <see cref="log"/>.<see cref="ILog.ForContext"/> with <paramref name="categoryName"/> used as context
        /// </para> 
        /// </summary>
        [NotNull]
        public ILogger CreateLogger([CanBeNull] string categoryName)
        {
            return new Logger(string.IsNullOrEmpty(categoryName) ? log : log.ForContext(categoryName));
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private class Logger : ILogger
        {
            private const string OriginalFormatKey = "{OriginalFormat}";
            private const string ScopeProperty = "Scope";

            private ILog log;
            private Scope currentScope;

            public Logger(ILog log)
            {
                this.log = log;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.None)
                    return;

                var translatedLevel = TranslateLogLevel(logLevel);
                if (!log.IsEnabledFor(translatedLevel))
                    return;

                var messageTemplate = ExtractMessageTemplate(state, exception, formatter);
                var logEvent = EnrichWithProperties(new LogEvent(translatedLevel, DateTimeOffset.Now, messageTemplate, exception), eventId, state);
                log.Log(logEvent);
            }

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && log.IsEnabledFor(TranslateLogLevel(logLevel));

            public IDisposable BeginScope<TState>(TState state)
            {
                var prevLog = log;
                var scopeValue = ReferenceEquals(state, null) ? typeof(TState).FullName : Convert.ToString(state);
                var scope = new Scope(this, prevLog, currentScope, scopeValue);

                var properties = new Dictionary<string, object>();
                if (currentScope == null)
                    properties.Add(ScopeProperty, scopeValue);
                else
                {
                    var scopePropertyValue = new List<string>();
                    for (var s = scope; s != null; s = s.Parent)
                        scopePropertyValue.Add(s.ScopeValue);
                    scopePropertyValue.Reverse();
                    properties.Add(ScopeProperty, scopePropertyValue);
                }

                if (state is IEnumerable<KeyValuePair<string, object>> props)
                {
                    foreach (var kvp in props)
                    {
                        if (kvp.Key != OriginalFormatKey)
                            properties[kvp.Key] = kvp.Value;
                    }
                }

                log = log.WithProperties(properties);
                currentScope = scope;
                return currentScope;
            }

            private static LogEvent EnrichWithProperties<TState>(LogEvent logEvent, EventId eventId, TState state)
            {
                if (state is IEnumerable<KeyValuePair<string, object>> props)
                {
                    foreach (var kvp in props)
                    {
                        if (kvp.Key == OriginalFormatKey)
                            continue;
                        logEvent = logEvent.WithPropertyIfAbsent(kvp.Key, kvp.Value);
                    }
                }

                if (eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name))
                    logEvent = logEvent.WithPropertyIfAbsent("EventId", eventId);
                        
                return logEvent;
            }

            private static string ExtractMessageTemplate<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state is IEnumerable<KeyValuePair<string, object>> props)
                {
                    foreach (var kvp in props)
                    {
                        if (kvp.Key == OriginalFormatKey)
                            return Convert.ToString(kvp.Value);
                    }
                }

                if (formatter != null)
                    return formatter(state, exception);

                return ReferenceEquals(state, null) ? typeof(TState).FullName : Convert.ToString(state);
            }

            private Abstractions.LogLevel TranslateLogLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        return Abstractions.LogLevel.Debug;
                    case LogLevel.Information:
                        return Abstractions.LogLevel.Info;
                    case LogLevel.Warning:
                        return Abstractions.LogLevel.Warn;
                    case LogLevel.Error:
                        return Abstractions.LogLevel.Error;
                    case LogLevel.Critical:
                        return Abstractions.LogLevel.Fatal;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }

            private class Scope : IDisposable
            {
                public readonly Scope Parent;
                public readonly string ScopeValue;
                private readonly Logger owner;
                private readonly ILog prevLog;

                public Scope(Logger owner, ILog prevLog, Scope parent, string scopeValue)
                {
                    this.owner = owner;
                    this.prevLog = prevLog;
                    Parent = parent;
                    ScopeValue = scopeValue;
                }

                public void Dispose()
                {
                    owner.log = prevLog;
                    owner.currentScope = Parent;
                }
            }
        }
    }
}