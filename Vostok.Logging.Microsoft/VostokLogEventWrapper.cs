using System.Collections;
using System.Collections.Generic;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft.Helpers;

namespace Vostok.Logging.Microsoft
{
    internal sealed class VostokLogEventWrapper : IEnumerable<KeyValuePair<string, object>>
    {
        public VostokLogEventWrapper(LogEvent logEvent)
        {
            LogEvent = logEvent;
        }

        public LogEvent LogEvent { get; }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object>(MicrosoftLogProperties.OriginalFormatKey, LogEvent.MessageTemplate);

            if (LogEvent.Properties == null)
                yield break;

            foreach (var property in LogEvent.Properties)
                yield return property;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}