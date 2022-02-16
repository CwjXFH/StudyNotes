using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EFCoreSlowQuery
{
    internal class EFCoreObserver : IObserver<DiagnosticListener>
    {
        private readonly SlowQueryObserver _slowQueryObserver;

        public EFCoreObserver(SlowQueryObserver slowQueryObserver)
        {
            _slowQueryObserver = slowQueryObserver;
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DbLoggerCategory.Name)
            {
                value.Subscribe(_slowQueryObserver);
            }
        }

        #region ignores
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
        #endregion
    }

#if NETSTANDARD2_0
    internal class SlowQueryObserver : IObserver<KeyValuePair<string, object>>
#else
        internal class SlowQueryObserver : IObserver<KeyValuePair<string, object?>>
#endif
    {
        private readonly ILogger _logger;
        private readonly EFCoreSlowQueryOptions _options;

        public SlowQueryObserver(ILogger logger, EFCoreSlowQueryOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "An exception occurred.");
        }

#if NETSTANDARD2_0
        public void OnNext(KeyValuePair<string, object> value)
#else
            public void OnNext(KeyValuePair<string, object?> value)
#endif
        {
            if (value.Key == RelationalEventId.CommandExecuted.Name
                && value.Value is CommandExecutedEventData eventData
                && eventData.Duration.Milliseconds > _options.SlowQueryThresholdMilliseconds)
            {
                if (_options.RecordSlowQueryLog)
                {
                    RecordSlowQueryLog(eventData);
                }
            }
            else if (value.Key == RelationalEventId.CommandError.Name)
            {
                RecordErrorCommand(value.Value);
            }
        }

        #region private
        private void RecordSlowQueryLog(CommandExecutedEventData eventData)
        {
            var msg = $"[EFCoreSlowQuery] duration: {eventData.Duration.Milliseconds}{Environment.NewLine}service: {_options.ServiceName}{Environment.NewLine}SQL: {eventData.Command.CommandText}";
            _logger.LogWarning(msg);
        }

        private void RecordErrorCommand(object value)
        {
            if (value is CommandErrorEventData errorEventData)
            {
                _logger.LogError(errorEventData.Exception,
                    $"Exec SQL error, SQL: {errorEventData.Command.CommandText}");
            }
            else
            {
                _logger.LogError("Exec SQL error, and no SQL is captured.");
            }
        }
        #endregion

        #region ignores
        public void OnCompleted()
        {
        }
        #endregion

    }
}
