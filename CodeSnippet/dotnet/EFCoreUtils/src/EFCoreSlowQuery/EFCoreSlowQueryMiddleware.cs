/*
 * Implementation EFCore slow query trace based on .NET diagnostic functions.
 * 
 * References:
 * https://docs.microsoft.com/en-us/ef/core/logging-events-diagnostics/
 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md
 */

// .NET6 support implicit global using
// reference: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive

#if !NET6_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#endif

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCoreExtensions.Middlewares
{
    /// <summary>
    /// Record EFCore slow query log
    /// </summary>
    public class EFCoreSlowQueryMiddleware
    {
        private readonly RequestDelegate _next;

        public EFCoreSlowQueryMiddleware(RequestDelegate next, ILogger<EFCoreSlowQueryMiddleware> logger, IOptionsMonitor<EFCoreSlowQueryOptions> optionsMonitor)
        {
            _next = next;

            var currentOptions = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(opt =>
            {
                currentOptions.ServiceName = opt.ServiceName;
                currentOptions.RecordSlowQueryLog = opt.RecordSlowQueryLog;
                currentOptions.SlowQueryThresholdMilliseconds = opt.SlowQueryThresholdMilliseconds;
                currentOptions.SlowQueryThresholdMilliseconds = opt.SlowQueryThresholdMilliseconds;
            });

            RegisterObserver(logger, currentOptions);
        }

        public EFCoreSlowQueryMiddleware(RequestDelegate next, ILogger<EFCoreSlowQueryMiddleware> logger, Action<EFCoreSlowQueryOptions> optionsAction)
        {
            _next = next;

            if (optionsAction == null)
            {
                throw new ArgumentNullException();
            }
            var options = new EFCoreSlowQueryOptions();
            optionsAction.Invoke(options);

            RegisterObserver(logger, options);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }

        private void RegisterObserver(ILogger logger, EFCoreSlowQueryOptions options)
        {
            var slowQueryObserver = new SlowQueryObserver(logger, options);
            DiagnosticListener.AllListeners.Subscribe(new EFCoreObserver(slowQueryObserver));
        }

        private class EFCoreObserver : IObserver<DiagnosticListener>
        {
            private readonly SlowQueryObserver _slowQueryObserver;

            public EFCoreObserver(SlowQueryObserver slowQueryObserver)
            {
                _slowQueryObserver = slowQueryObserver;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(DiagnosticListener value)
            {
                if (value.Name == DbLoggerCategory.Name)
                {
                    value.Subscribe(_slowQueryObserver);
                }
            }
        }

#if NETSTANDARD2_0
        private class SlowQueryObserver : IObserver<KeyValuePair<string, object>>
#else
        private class SlowQueryObserver : IObserver<KeyValuePair<string, object?>>
#endif
        {
            /// <summary>
            /// A tag used in log.
            /// </summary>
            private const string EFCoreSlowQueryTag = "[EFCoreSlowQuery]";

            private readonly ILogger _logger;
            private readonly EFCoreSlowQueryOptions _options;

            public SlowQueryObserver(ILogger logger, EFCoreSlowQueryOptions options)
            {
                _logger = logger;
                _options = options;
            }

            public void OnCompleted()
            {
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

            private void RecordSlowQueryLog(CommandExecutedEventData eventData)
            {
                var msg = $"{EFCoreSlowQueryTag} service: {_options.ServiceName} duration: {eventData.Duration.Milliseconds} {Environment.NewLine}SQL: {eventData.Command.CommandText}";
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
        }
    }

    public static class EFCoreSlowQueryMiddlewareExtension
    {
        public static IApplicationBuilder UseEFCoreSlowQuery(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EFCoreSlowQueryMiddleware>();
        }

        public static IApplicationBuilder UseEFCoreSlowQuery(this IApplicationBuilder builder,
            Action<EFCoreSlowQueryOptions> optionsAction)
        {
            return builder.UseMiddleware<EFCoreSlowQueryMiddleware>(optionsAction);
        }
    }

    public class EFCoreSlowQueryOptions
    {
        /// <summary>
        /// The section name in configures file, like appsettings.json.
        /// </summary>
        public const string OptionsName = "EFCoreSlowQuery";

        public string ServiceName { set; get; } = "";

        private int _slowQueryThresholdMilliseconds;

        public int SlowQueryThresholdMilliseconds
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _slowQueryThresholdMilliseconds = value;
            }
            get => _slowQueryThresholdMilliseconds;
        }

        public bool RecordSlowQueryLog { set; get; } = true;
    }
}