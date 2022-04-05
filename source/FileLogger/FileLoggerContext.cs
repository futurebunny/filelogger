#define IRL_INCLUDE_EXTENSIONS 
#define IRL_SPRINT_4_OR_LATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Karambolo.Extensions.Logging.File
{
    public class FileLoggerContext
    {
        public static readonly FileLoggerContext Default = new FileLoggerContext(default);

        public FileLoggerContext(CancellationToken completeToken)
            : this(completeToken, null, null) { }

        public FileLoggerContext(CancellationToken completeToken, TimeSpan? completionTimeout = null, TimeSpan? writeRetryDelay = null)
        {
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
            FileLoggerContextCreated = DateTime.UtcNow;

            Debugging.TraceMethodEntry(false, $">> Begin Creating FileLoggerContext() at: {FileLoggerContextCreated.ToLocalTime()}, equal to [0x{FileLoggerContextCreated.Ticks.ToString("X8")}]");
#endif
            CompleteToken = completeToken;
            CompletionTimeout = completionTimeout ?? TimeSpan.FromMilliseconds(1500);
            WriteRetryDelay = writeRetryDelay ?? TimeSpan.FromMilliseconds(500);

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
            Debugging.TraceMethodExit(false, $"<< Done Creating FileLoggerContext() at: {FileLoggerContextCreated.ToLocalTime()}, equal to [0x{FileLoggerContextCreated.Ticks.ToString("X8")}]");
#endif
        }

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
        public DateTime FileLoggerContextCreated { get; init; }
#endif

        public CancellationToken CompleteToken { get; }

        public TimeSpan CompletionTimeout { get; }

        public TimeSpan WriteRetryDelay { get; }

        public event Action<IFileLoggerDiagnosticEvent> DiagnosticEvent;

        internal void ReportDiagnosticEvent<TEvent>(in TEvent @event)
            where TEvent : struct, IFileLoggerDiagnosticEvent
        {
            DiagnosticEvent?.Invoke(@event);
        }

        public virtual DateTimeOffset GetTimestamp() => DateTimeOffset.UtcNow;

        internal IEnumerable<FileLoggerProvider> GetProviders(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .OfType<FileLoggerProvider>()
                .Where(provider => provider.Context == this);
        }

        public Task GetCompletion(IServiceProvider serviceProvider)
        {
            return Task.WhenAll(GetProviders(serviceProvider).Select(provider => provider.Completion));
        }
    }
}
