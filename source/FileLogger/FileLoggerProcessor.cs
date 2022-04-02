#define IRL_INCLUDE_EXTENSIONS 
#define IRL_SPRINT_3_OR_LATER
#define IRL_SPRINT_4_OR_LATER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

#if IRL_INCLUDE_EXTENSIONS
using Intemporal.Experimental.Diagnostics.Logging;
#endif

namespace Karambolo.Extensions.Logging.File
{
    public interface IFileLoggerProcessor : IDisposable
    {
        Task Completion { get; }

        void Enqueue(FileLogEntry entry, ILogFileSettings fileSettings, IFileLoggerSettings settings);

        Task ResetAsync(Action onQueuesCompleted = null);
        Task CompleteAsync();
    }

    public partial class FileLoggerProcessor : IFileLoggerProcessor
    {
        private enum Status
        {
            Running,
            Completing,
            Completed,
        }

        protected internal partial class LogFileInfo
        {
            private Stream _appendStream;

            public LogFileInfo(FileLoggerProcessor processor, ILogFileSettings fileSettings, IFileLoggerSettings settings)
            {
                bool fExceptionsHit = true;
                try
                {

#if IRL_INCLUDE_EXTENSIONS && (IRL_SPRINT_3_OR_LATER || IRL_SPRINT_4_OR_LATER)
                    DateTime dtCreateLogFile = DateTime.UtcNow.ToLocalTime();
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Start LogFileInfo.ctor() at: {dtCreateLogFile.ToLocalTime().ToString("O")}, equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}]");
#if IRL_SPRINT_4_OR_LATER
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Updating dtCreateLogFile = processor.Context.FileLoggerContextCreated.ToLocalTime()");
                    dtCreateLogFile = processor.Context.FileLoggerContextCreated.ToLocalTime();

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("O")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Updating LogFileCreationTimestamp = processor.Context.FileLoggerContextCreated.ToLocalTime()");
                    LogFileCreationTimestamp = processor.Context.FileLoggerContextCreated.ToLocalTime();

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {LogFileCreationTimestamp.ToLocalTime().ToString("O")} equal to [0x{LogFileCreationTimestamp.Ticks.ToString("X8")}] Ticks");

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Updating DateTime firstCreationDate = fileSettings.GetFileCreationDate()");
                    DateTime firstCreationDate = fileSettings.GetFileCreationDate();

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {firstCreationDate.ToLocalTime().ToString("O")} equal to [0x{firstCreationDate.Ticks.ToString("X8")}] Ticks");
#endif
#endif

                    BasePath = settings.BasePath ?? string.Empty;
                    PathFormat = fileSettings.Path;
                    PathPlaceholderResolver = GetActualPathPlaceholderResolver(fileSettings.PathPlaceholderResolver ?? settings.PathPlaceholderResolver);
                    FileAppender = settings.FileAppender ?? processor._fallbackFileAppender.Value;
                    AccessMode = fileSettings.FileAccessMode ?? settings.FileAccessMode ?? LogFileAccessMode.Default;
                    Encoding = fileSettings.FileEncoding ?? settings.FileEncoding ?? Encoding.UTF8;
                    DateFormat = fileSettings.DateFormat ?? settings.DateFormat ?? "dt<date:yyyy>-<date:MM>-<date:dd>--<date:HH>-<date:mm>-<date:ss>--";
                    CounterFormat = fileSettings.CounterFormat ?? settings.CounterFormat;
                    MaxSize = fileSettings.MaxFileSize ?? settings.MaxFileSize ?? 0;

#if IRL_INCLUDE_EXTENSIONS && (IRL_SPRINT_3_OR_LATER || IRL_SPRINT_4_OR_LATER)
                    UseTickCount = fileSettings.UseTickCount ?? false;

                    TickCountFormat = fileSettings.TickCountFormat ?? "X8";
#if IRL_SPRINT_4_OR_LATER
                    //
                    // Now we should just use the LogFileCreationTimestamp.Ticks in the formatting code
                    //
                    TickCount = LogFileCreationTimestamp.Ticks;

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for TickCount was updated to {LogFileCreationTimestamp.ToLocalTime().ToString("O")} equal to [0x{LogFileCreationTimestamp.Ticks.ToString("X8")}] Ticks");

                    long lTickCountCommon = fileSettings.GetFileCreationTickCount();

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for TickCount was updated to equal to [0x{lTickCountCommon.ToString("X8")}] Ticks");

#else
                TickCount = dtCreateLogFile.Ticks;
#endif
#endif

                    //
                    // This is async, so the actual file creation time will differ from the internal reference time
                    //
                    Queue = processor.CreateLogFileQueue(fileSettings, settings);

                    // important: closure must pick up the current token!
                    CancellationToken forcedCompleteToken = processor._forcedCompleteTokenSource.Token;
                    WriteFileTask = Task.Run(() => processor.WriteFileAsync(this, forcedCompleteToken));

                    static LogFilePathPlaceholderResolver GetActualPathPlaceholderResolver(LogFilePathPlaceholderResolver resolver) =>
                        resolver == null ?
                        s_defaultPathPlaceholderResolver :
                        (placeholderName, inlineFormat, context) => resolver(placeholderName, inlineFormat, context) ?? s_defaultPathPlaceholderResolver(placeholderName, inlineFormat, context);


#if IRL_INCLUDE_EXTENSIONS && (IRL_SPRINT_3_OR_LATER || IRL_SPRINT_4_OR_LATER)
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Done LogFileInfo.ctor() at: {dtCreateLogFile.ToLocalTime().ToString("O")}, equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}]");
#if IRL_SPRINT_4_OR_LATER
                    // Display both...
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Done LogFileInfo.ctor() at: {LogFileCreationTimestamp.ToLocalTime().ToString("O")}, equal to [0x{LogFileCreationTimestamp.Ticks.ToString("X8")}]");
#endif
#endif


                    fExceptionsHit = false;
                }
                catch (System.Exception ex)
                {
                    fExceptionsHit = true;

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                    if (null != ex.StackTrace)
                    {
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                    }
                }
                finally
                {
                    if (fExceptionsHit)
                    {
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(fExceptionsHit, $"LogFilePathFormatContext.ctor() - unable to safely generate log file name");
                    }
                }
            }

            public string BasePath { get; }
            public string PathFormat { get; }
            public LogFilePathPlaceholderResolver PathPlaceholderResolver { get; }
            public IFileAppender FileAppender { get; }
            public LogFileAccessMode AccessMode { get; }
            public Encoding Encoding { get; }
            public string DateFormat { get; }
            public string CounterFormat { get; }

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
            public bool UseTickCount { get; }
            public long TickCount { get; }
            public string TickCountFormat { get; }
#endif

#if (IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER)
            public DateTime LogFileCreationTimestamp { get; }
#endif
            public long MaxSize { get; }

            public Channel<FileLogEntry> Queue { get; }
            public Task WriteFileTask { get; }

            public int Counter { get; set; }
            public string CurrentPath { get; set; }

            public bool IsOpen => _appendStream != null;
            public long? Size => _appendStream?.Length;

            internal bool ShouldEnsurePreamble { get; private set; }

            internal void Open(IFileInfo fileInfo)
            {
                _appendStream = FileAppender.CreateAppendStream(fileInfo);

                ShouldEnsurePreamble = true;
            }

            internal async ValueTask EnsurePreambleAsync(CancellationToken cancellationToken)
            {
                if (_appendStream.Length == 0)
                    await WriteBytesAsync(Encoding.GetPreamble(), cancellationToken).ConfigureAwait(false);

                ShouldEnsurePreamble = false;
            }

            internal void Flush()
            {
                // FlushAsync is extremely slow currently
                // https://github.com/dotnet/corefx/issues/32837
                _appendStream.Flush();
            }

            internal void Close()
            {
                Stream appendStream = _appendStream;
                _appendStream = null;
                appendStream.Dispose();
            }
        }

        protected class LogFilePathFormatContext : ILogFilePathFormatContext
        {
            private readonly FileLoggerProcessor _processor;
            private readonly LogFileInfo _logFile;
            private readonly FileLogEntry _logEntry;
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
            private readonly DateTime _logInitializedTimestamp;
#endif


            public LogFilePathFormatContext(FileLoggerProcessor processor, LogFileInfo logFile, FileLogEntry logEntry)
            {
                bool fExceptionsHit = true;
                try
                {
#if IRL_INCLUDE_EXTENSIONS
#if IRL_SPRINT_4_OR_LATER
                    DateTime dtCreateLogFile = logFile.LogFileCreationTimestamp.ToLocalTime(); 

                    //DateTime dtCreateLogFile = _logInitializedTimestamp;
                    //_logInitializedTimestamp = logFile.LogFileCreationTimestamp;

                    string szTicks = $"{dtCreateLogFile.Ticks.ToString("X8")}";
                    string szTime = dtCreateLogFile.ToLocalTime().ToString("O");
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $"LogFilePathFormatContext.ctor() - Begin Creating Log File at: {szTime}, equal to [0x{szTicks}]");

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToStringUtcInvariant()} equal to [0x{dtCreateLogFile.ToStringHexFullLength()}] Ticks");
#endif
#endif

                    _processor = processor;
                    _logFile = logFile;
                    _logEntry = logEntry;

#if IRL_INCLUDE_EXTENSIONS
#if IRL_SPRINT_4_OR_LATER
                    // Now already done above at function entrance
                    _logInitializedTimestamp = _logFile.LogFileCreationTimestamp;

                    szTicks = $"{_logInitializedTimestamp.Ticks.ToString("X8")}";
                    szTime = _logInitializedTimestamp.ToLocalTime().ToString("O");

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for _logInitializedTimestamp was initialized to {szTime} equal to [0x{szTicks}] Ticks");

                    dtCreateLogFile = _logInitializedTimestamp.ToLocalTime();

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToStringUtcInvariant()} equal to [0x{dtCreateLogFile.ToStringHexFullLength()}] Ticks");

                    //Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("O")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

                    //Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("o")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

                    
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"LogFilePathFormatContext.ctor() - Begin Creating Log File at: {_logInitializedTimestamp.ToLocalTime().ToString("O")} , equal to [0x{_logInitializedTimestamp.Ticks.ToString("X8")}]");

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"LogFilePathFormatContext.ctor() - Begin Creating Log File at: {_logInitializedTimestamp.ToStringUtcInvariant()} , equal to [0x{_logInitializedTimestamp.ToStringHexFullLength()}]");

#elif IRL_SPRINT_3_OR_LATER

                    _logInitializedTimestamp = _logFile.LogFileCreationTimestamp;
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"Begin Creating Log File at: {_logInitializedTimestamp.ToLocalTime()} , equal to [0x{_logInitializedTimestamp.Ticks.ToString("X8")}]");
#endif
#endif
                    fExceptionsHit = false;
                }
                catch (System.Exception ex)
                {
                    fExceptionsHit = true;

                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                    if (null != ex.StackTrace)
                    {
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                    }
                }
                finally
                {
                    if (fExceptionsHit)
                    {
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(fExceptionsHit, $"LogFilePathFormatContext.ctor() - unable to safely generate log file name");
                    }
                }

            }

            FileLogEntry ILogFilePathFormatContext.LogEntry => _logEntry;

            string ILogFilePathFormatContext.DateFormat => _logFile.DateFormat;
            string ILogFilePathFormatContext.CounterFormat => _logFile.CounterFormat;
            int ILogFilePathFormatContext.Counter => _logFile.Counter;

            string ILogFilePathFormatContext.FormatDate(string inlineFormat) => _processor.GetDate(inlineFormat, _logFile, _logEntry);
            string ILogFilePathFormatContext.FormatCounter(string inlineFormat) => _processor.GetCounter(inlineFormat, _logFile, _logEntry);

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
            string ILogFilePathFormatContext.FormatTickCount(string inlineFormat) => _processor.GetTickCount(inlineFormat, _logFile, _logEntry);
#endif

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_5_OR_LATER
            string ILogFilePathFormatContext.FormatTime(string inlineFormat) => _processor.GetTickCount(inlineFormat, _logFile, _logEntry);
#endif
            public string ResolvePlaceholder(Match match)
            {
                var placeholderName = match.Groups[1].Value;
                var inlineFormat = match.Groups[2].Value;

                return
                    _logFile.PathPlaceholderResolver(placeholderName, inlineFormat.Length > 0 ? inlineFormat : null, this) ??
                    match.Groups[0].Value;
            }
        }

        private static readonly LogFilePathPlaceholderResolver s_defaultPathPlaceholderResolver = (placeholderName, inlineFormat, context) =>
        {
#if DEBUG
            try
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, placeholderName);

                switch (placeholderName)
                {
                    case "date":
                        {
                            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, "date");
                            return context.FormatDate(inlineFormat);
                        }
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
                    case "time":
                        {
                            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, "time");
                            return context.FormatDate(inlineFormat);
                        }
#endif
                    case "counter":
                        {
                            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, "counter");
                            return context.FormatCounter(inlineFormat);
                        }
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
                    case "ticks":
                        {
                            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, "ticks");
                            return context.FormatTickCount(inlineFormat);
                        }
#endif
                    default:
                        {
                            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(true, "default");
                            return null;
                        }
                }
            }
            catch (System.Exception ex)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting log file path placeholder resulted in an unhandled exception");
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                if (null != ex.StackTrace)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                }
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(true, "EXCEPTION");
                return null;
            }
#else
            switch (placeholderName)
            {
                case "date": return context.FormatDate(inlineFormat);
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
                case "time": return context.FormatDate(inlineFormat);
#endif
                case "counter": return context.FormatCounter(inlineFormat);
#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
                case "ticks": return context.FormatTickCount(inlineFormat);
#endif
                default: return null;
            }
#endif
        };

        private static readonly Regex s_pathPlaceholderRegex = new Regex(@"<([_a-zA-Z][_a-zA-Z0-9-]*)(?::\s*([^<>]*[^\s<>]))?>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Lazy<char[]> s_invalidPathChars = new Lazy<char[]>(() => Path.GetInvalidPathChars()
            .Concat(Path.GetInvalidFileNameChars())
            .Except(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar })
            .ToArray());

        private readonly Lazy<PhysicalFileAppender> _fallbackFileAppender;
        private readonly Dictionary<ILogFileSettings, LogFileInfo> _logFiles;
        private readonly TaskCompletionSource<object> _completeTaskCompletionSource;
        private readonly CancellationTokenRegistration _completeTokenRegistration;
        private CancellationTokenSource _forcedCompleteTokenSource;
        private Status _status;

        public FileLoggerProcessor(FileLoggerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;

            _fallbackFileAppender = new Lazy<PhysicalFileAppender>(() => new PhysicalFileAppender(Environment.CurrentDirectory));

            _logFiles = new Dictionary<ILogFileSettings, LogFileInfo>();

            _completeTaskCompletionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _forcedCompleteTokenSource = new CancellationTokenSource();

            _completeTokenRegistration = context.CompleteToken.Register(Complete, useSynchronizationContext: false);
        }

        public void Dispose()
        {
            lock (_logFiles)
                if (_status != Status.Completed)
                {
                    _completeTokenRegistration.Dispose();

                    _forcedCompleteTokenSource.Cancel();
                    _forcedCompleteTokenSource.Dispose();

                    _completeTaskCompletionSource.TrySetResult(null);

                    if (_fallbackFileAppender.IsValueCreated)
                        _fallbackFileAppender.Value.Dispose();

                    DisposeCore();

                    _status = Status.Completed;
                }
        }

        protected virtual void DisposeCore() { }

        public FileLoggerContext Context { get; }

        public Task Completion => _completeTaskCompletionSource.Task;

        private async Task ResetCoreAsync(Action onQueuesCompleted, bool complete)
        {
            CancellationTokenSource forcedCompleteTokenSource;
            Task[] completionTasks;

            lock (_logFiles)
            {
                if (_status != Status.Running)
                    return;

                forcedCompleteTokenSource = _forcedCompleteTokenSource;
                _forcedCompleteTokenSource = new CancellationTokenSource();

                completionTasks = _logFiles.Values.Select(async logFile =>
                {
                    logFile.Queue.Writer.Complete();

                    await logFile.WriteFileTask.ConfigureAwait(false);

                    if (logFile.IsOpen)
                        logFile.Close();
                }).ToArray();

                _logFiles.Clear();

                onQueuesCompleted?.Invoke();

                if (complete)
                    _status = Status.Completing;
            }

            try
            {
                var completionTimeoutTask = Task.Delay(Context.CompletionTimeout);
                if ((await Task.WhenAny(Task.WhenAll(completionTasks), completionTimeoutTask).ConfigureAwait(false)) == completionTimeoutTask)
                    Context.ReportDiagnosticEvent(new FileLoggerDiagnosticEvent.QueuesCompletionForced(this));

                forcedCompleteTokenSource.Cancel();
                forcedCompleteTokenSource.Dispose();
            }
            finally
            {
                if (complete)
                    Dispose();
            }
        }

        public Task ResetAsync(Action onQueuesCompleted = null)
        {
            return ResetCoreAsync(onQueuesCompleted, complete: false);
        }

        public Task CompleteAsync()
        {
            return ResetCoreAsync(null, complete: true);
        }

        private async void Complete()
        {
            await CompleteAsync().ConfigureAwait(false);
        }

        protected virtual LogFileInfo CreateLogFile(ILogFileSettings fileSettings, IFileLoggerSettings settings)
        {
#if IRL_INCLUDE_EXTENSIONS && (IRL_SPRINT_3_OR_LATER || IRL_SPRINT_4_OR_LATER)
            DateTime dtCreateLogFile = DateTime.UtcNow;

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("O")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

            //
            // In Sprint 4 - This is where we should pull the value from the processor and context..
            //
            dtCreateLogFile = dtCreateLogFile.ToLocalTime();
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> CreateLogFile() - Begin Creating Log File at: {dtCreateLogFile.ToLocalTime()}, equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}]");

            // redo this because the tracing method output might be causing a slowdown...
            dtCreateLogFile = DateTime.UtcNow.ToLocalTime();

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("O")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

#if IRL_SPRINT_4_OR_LATER
            dtCreateLogFile = this.Context.FileLoggerContextCreated.ToLocalTime();

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to {dtCreateLogFile.ToLocalTime().ToString("O")} equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

            DateTime firstCreationDate = fileSettings.GetFileCreationDate();

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for firstCreationDate was updated to {firstCreationDate.ToLocalTime().ToString("O")} equal to [0x{firstCreationDate.Ticks.ToString("X8")}] Ticks");
#endif

#if false && IRL_SPRINT_4_OR_LATER

            //
            // In Sprint 4 - This is where we should pull the value from the processor and context..
            //
            dtCreateLogFile = dtCreateLogFile.ToLocalTime();
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Begin Creating Log File at: {dtCreateLogFile.ToLocalTime()}, equal to [0x{dtCreateLogFile.Ticks.ToString("X8")}]");
            
            // redo this because the tracing method output might be causing a slowdown...
            dtCreateLogFile = DateTime.UtcNow.ToLocalTime(); 

            //
            // TODO - Should we just pass in the timestamp from this level....
            //
            LogFileInfo logFile = new LogFileInfo(this, fileSettings, settings);

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for dtCreateLogFile was updated to [0x{dtCreateLogFile.Ticks.ToString("X8")}] Ticks");

            //
            //
            //
            long lTicksDifference = logFile.LogFileCreationTimestamp.Ticks - dtCreateLogFile.Ticks;

            if (lTicksDifference == 0)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Log File Info Creation Timestamp exactly equals logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
            }
            else if (lTicksDifference > 0)
            {
                if (lTicksDifference < 20000)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Log File Info Creation Timestamp only differs from logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
                }
                else
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"WARNING: Log File Info Creation Timestamp greatly differs from logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
                }
            }
            else
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"WARNING: Log File Info Creation Timestamp differs by NEGATIVE TIME from logFile.LogFileCreationTimestamp by [{lTicksDifference.ToString("X8")}] Ticks");
            }

            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Done Creating Log File Info at: {logFile.LogFileCreationTimestamp.ToLocalTime()}, equal to [0x{logFile.LogFileCreationTimestamp.Ticks.ToString("X8")}]");

            return logFile;
#else

            //
            // TODO - Should we just pass in the timestamp from this level....
            //
            LogFileInfo logFile = new LogFileInfo(this, fileSettings, settings);

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Value for logFile.LogFileCreationTimestamp.ToLocalTime() was updated to [0x{logFile.LogFileCreationTimestamp.Ticks.ToString("X8")}] Ticks");

            //
            // Need to break here
            //
            long lTicksDifference = logFile.LogFileCreationTimestamp.Ticks - dtCreateLogFile.Ticks;

            if (lTicksDifference == 0)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Log File Info Creation Timestamp exactly equals logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
            }
            else if (lTicksDifference > 0)
            {
                if (lTicksDifference < 20000)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Log File Info Creation Timestamp only differs from logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
                }
                else
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"WARNING: Log File Info Creation Timestamp greatly differs from logFile.LogFileCreationTimestamp by [0x{lTicksDifference.ToString("X8")}] Ticks");
                }
            }
            else
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"WARNING: Log File Info Creation Timestamp differs by NEGATIVE TIME from logFile.LogFileCreationTimestamp by [{lTicksDifference.ToString("X8")}] Ticks");
            }

            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< CreateLogFile() - Done Creating Log File Info at: {logFile.LogFileCreationTimestamp.ToLocalTime()}, equal to [0x{logFile.LogFileCreationTimestamp.Ticks.ToString("X8")}]");

            return logFile;
#endif

#else
            return new LogFileInfo(this, fileSettings, settings);
#endif
        }

        protected virtual Channel<FileLogEntry> CreateLogFileQueue(ILogFileSettings fileSettings, IFileLoggerSettings settings)
        {
            var maxQueueSize = fileSettings.MaxQueueSize ?? settings.MaxQueueSize ?? 0;

            return
                maxQueueSize > 0 ?
                Channel.CreateBounded<FileLogEntry>(ConfigureChannelOptions(new BoundedChannelOptions(maxQueueSize)
                {
                    FullMode = BoundedChannelFullMode.DropWrite
                })) :
                Channel.CreateUnbounded<FileLogEntry>(ConfigureChannelOptions(new UnboundedChannelOptions()));

            static TOptions ConfigureChannelOptions<TOptions>(TOptions options) where TOptions : ChannelOptions
            {
                options.AllowSynchronousContinuations = false;
                options.SingleReader = true;
                options.SingleWriter = false;
                return options;
            }
        }

        public void Enqueue(FileLogEntry entry, ILogFileSettings fileSettings, IFileLoggerSettings settings)
        {
            LogFileInfo logFile;

            lock (_logFiles)
            {
                if (_status == Status.Completed)
                    throw new ObjectDisposedException(nameof(FileLoggerProcessor));

                if (_status != Status.Running)
                    return;

                if (!_logFiles.TryGetValue(fileSettings, out logFile))
                    _logFiles.Add(fileSettings, logFile = CreateLogFile(fileSettings, settings));
            }

            if (!logFile.Queue.Writer.TryWrite(entry))
                Context.ReportDiagnosticEvent(new FileLoggerDiagnosticEvent.LogEntryDropped(this, logFile, entry));
        }

        protected virtual string GetDate(string inlineFormat, LogFileInfo logFile, FileLogEntry entry)
        {
            string? szValue = null;

#if DEBUG
            try
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, "Creating Date Fragment");

                string szFormat = inlineFormat ?? logFile.DateFormat;

                if (!String.IsNullOrEmpty(szFormat))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: String Format Fragment [{szFormat}]");
                }

                //
                // This appears to pull the date and time from the logging message timestamp...which results in dozens -> hundreds -> thousands of log files  being created...
                //
                szValue = entry.Timestamp.ToLocalTime().ToString(inlineFormat ?? logFile.DateFormat, CultureInfo.InvariantCulture);

                if (!String.IsNullOrEmpty(szValue))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: (From LogEntry.TimestampString) Format Result Fragment [{szValue}]");
                }

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_4_OR_LATER
                string szValue2 = logFile.LogFileCreationTimestamp.ToLocalTime().ToString(inlineFormat ?? logFile.DateFormat, CultureInfo.InvariantCulture);

                if (!String.IsNullOrEmpty(szValue2))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: (From LogFile.CreationDate) Format Result Fragment [{szValue2}]");

                    if (szValue != szValue2)
                    {
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: OVERRIDING LOG ENTRY DATE WITH FILE CREATION DATE) Format Result Fragment [{szValue2}]");
                        szValue = szValue2;
                    }
                }
#else
                return szValue;
#endif

            }
            catch (System.Exception ex)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                if (null != ex.StackTrace)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                }
                szValue = String.Empty;
            }
            finally
            {
                if (null == szValue)
                {
                    szValue = String.Empty;
                }
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"Returning Date Fragment [{szValue}]");
            }
#else
            //return  entry.Timestamp.ToLocalTime().ToString(inlineFormat ?? logFile.DateFormat, CultureInfo.InvariantCulture);
            szValue = entry.Timestamp.ToLocalTime().ToString(inlineFormat ?? logFile.DateFormat, CultureInfo.InvariantCulture);
#endif
            return (string)szValue;
        }

        protected virtual string GetCounter(string inlineFormat, LogFileInfo logFile, FileLogEntry entry)
        {
            string? szValue = null;

#if DEBUG
            try
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, null);

                string szFormat = inlineFormat ?? logFile.CounterFormat;

                if (!String.IsNullOrEmpty(szFormat))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Format Format Fragment [{szFormat}]");
                }

                szValue = logFile.Counter.ToString(inlineFormat ?? logFile.CounterFormat, CultureInfo.InvariantCulture);

                if (!String.IsNullOrEmpty(szValue))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: String Format Result Fragment [{szValue}]");
                }
                return szValue;
            }
            catch (System.Exception ex)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                if (null != ex.StackTrace)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                }
                szValue = String.Empty;
            }
            finally
            {
                if (null == szValue)
                {
                    szValue = String.Empty;
                }
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"Returning Counter Fragment [{szValue}]");
            }
#else
            //return  logFile.Counter.ToString(inlineFormat ?? logFile.CounterFormat, CultureInfo.InvariantCulture);
            szValue = logFile.Counter.ToString(inlineFormat ?? logFile.CounterFormat, CultureInfo.InvariantCulture);
#endif
            return (string)szValue;
        }

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
        protected virtual string GetTickCount(string inlineFormat, LogFileInfo logFile, FileLogEntry entry)
        {
            string? szValue = null;
#if DEBUG
            try
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, null);

                string szFormat = inlineFormat ?? logFile.TickCountFormat;

                if (!String.IsNullOrEmpty(szFormat))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Format Format Fragment [{szFormat}]");
                }

                szValue = logFile.TickCount.ToString(inlineFormat ?? logFile.TickCountFormat, CultureInfo.InvariantCulture);

                if (!String.IsNullOrEmpty(szValue))
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: String Format Result Fragment [{szValue}]");
                }
            }
            catch (System.Exception ex)
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: {ex.Message}");
                if (null != ex.StackTrace)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine(ex.StackTrace.ToString());
                }
                szValue = String.Empty;
            }
            finally
            {
                if (null == szValue)
                {
                    szValue = String.Empty;
                }
                Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"Returning TickCount Fragment [{szValue}]");
            }
#else
            //return logFile.TickCount.ToString(inlineFormat ?? logFile.TickCountFormat, CultureInfo.InvariantCulture);
            szValue = logFile.TickCount.ToString(inlineFormat ?? logFile.TickCountFormat, CultureInfo.InvariantCulture);
#endif
            return (string)szValue;

        }
#endif

        protected virtual bool CheckFileSize(string filePath, LogFileInfo logFile, FileLogEntry entry)
        {
            long currentFileSize;
            if (!logFile.IsOpen || logFile.CurrentPath != filePath)
            {
                IFileInfo fileInfo = logFile.FileAppender.FileProvider.GetFileInfo(Path.Combine(logFile.BasePath, filePath));

                if (!fileInfo.Exists)
                    return true;

                if (fileInfo.IsDirectory)
                    return false;

                currentFileSize = fileInfo.Length;
            }
            else
            {
                if ((logFile.IsOpen) && (null != logFile.Size))
                {
                    currentFileSize = logFile.Size.Value;
                }
                else
                {
                    currentFileSize = 0;
                }
            }

            long expectedFileSize = currentFileSize > 0 ? currentFileSize : logFile.Encoding.GetPreamble().Length;
            expectedFileSize += logFile.Encoding.GetByteCount(entry.Text);

            return expectedFileSize <= logFile.MaxSize;
        }

        protected virtual string FormatFilePath(LogFileInfo logFile, FileLogEntry entry)
        {
            return s_pathPlaceholderRegex.Replace(logFile.PathFormat, new LogFilePathFormatContext(this, logFile, entry).ResolvePlaceholder);
        }

        protected virtual bool UpdateFilePath(LogFileInfo logFile, FileLogEntry entry, CancellationToken cancellationToken)
        {
            string filePath = FormatFilePath(logFile, entry);

            if (logFile.MaxSize > 0)
                while (!CheckFileSize(filePath, logFile, entry))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    logFile.Counter++;
                    var newFilePath = FormatFilePath(logFile, entry);

                    if (filePath == newFilePath)
                        break;

                    filePath = newFilePath;
                }

            if (logFile.CurrentPath == filePath)
                return false;

            logFile.CurrentPath = filePath;
            return true;
        }

        private async ValueTask WriteEntryAsync(LogFileInfo logFile, FileLogEntry entry, CancellationToken cancellationToken)
        {
            const int checkFileState = 0;
            const int tryOpenFileState = 1;
            const int retryOpenFileState = 2;
            const int writeState = 3;
            const int idleState = 4;

            int state = checkFileState;
            IFileInfo fileInfo = null;
            for (; ; )
                switch (state)
                {
                    case checkFileState:
                        try
                        {
                            if (UpdateFilePath(logFile, entry, cancellationToken) && logFile.IsOpen)
                                logFile.Close();

                            if (!logFile.IsOpen)
                            {
                                // GetFileInfo behavior regarding invalid filenames is inconsistent across .NET runtimes (and operating systems?)
                                // e.g. PhysicalFileProvider returns NotFoundFileInfo in .NET 5 but throws an exception in previous versions on Windows
                                fileInfo = logFile.FileAppender.FileProvider.GetFileInfo(Path.Combine(logFile.BasePath, logFile.CurrentPath));
                                state = tryOpenFileState;
                            }
                            else
                                state = writeState;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            ReportFailure(logFile, entry, ex);

                            // discarding entry when file path is invalid
                            if (logFile.CurrentPath.IndexOfAny(s_invalidPathChars.Value) >= 0)
                                return;

                            state = idleState;
                        }
                        break;
                    case tryOpenFileState:
                        try
                        {
                            logFile.Open(fileInfo);

                            state = writeState;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            state = retryOpenFileState;
                        }
                        break;
                    case retryOpenFileState:
                        try
                        {
                            await logFile.FileAppender.EnsureDirAsync(fileInfo, cancellationToken).ConfigureAwait(false);
                            logFile.Open(fileInfo);

                            state = writeState;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            ReportFailure(logFile, entry, ex);

                            // discarding entry when file path is invalid
                            if (logFile.CurrentPath.IndexOfAny(s_invalidPathChars.Value) >= 0)
                                return;

                            state = idleState;
                        }
                        break;
                    case writeState:
                        try
                        {
                            try
                            {
                                if (logFile.ShouldEnsurePreamble)
                                    await logFile.EnsurePreambleAsync(cancellationToken).ConfigureAwait(false);

                                // https://docs.microsoft.com/en-us/dotnet/api/system.io.file.writeallbytesasync?view=net-6.0

                                await logFile.WriteBytesAsync(logFile.Encoding.GetBytes(entry.Text), cancellationToken).ConfigureAwait(false);

                                if (logFile.AccessMode == LogFileAccessMode.KeepOpenAndAutoFlush)
                                    logFile.Flush();
                            }
                            finally
                            {
                                if (logFile.AccessMode == LogFileAccessMode.OpenTemporarily)
                                    logFile.Close();
                            }

                            return;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            ReportFailure(logFile, entry, ex);

                            state = idleState;
                        }
                        break;
                    case idleState:
                        // discarding failed entry on forced complete
                        if (Context.WriteRetryDelay > TimeSpan.Zero)
                            await Task.Delay(Context.WriteRetryDelay, cancellationToken).ConfigureAwait(false);
                        else
                            cancellationToken.ThrowIfCancellationRequested();

                        state = checkFileState;
                        break;
                }

            void ReportFailure(LogFileInfo logFile, FileLogEntry entry, Exception exception)
            {
                Context.ReportDiagnosticEvent(new FileLoggerDiagnosticEvent.LogEntryWriteFailed(this, logFile, entry, exception));
            }
        }

        private async Task WriteFileAsync(LogFileInfo logFile, CancellationToken cancellationToken)
        {
            ChannelReader<FileLogEntry> queue = logFile.Queue.Reader;
            while (await queue.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                while (queue.TryRead(out FileLogEntry entry))
                    await WriteEntryAsync(logFile, entry, cancellationToken).ConfigureAwait(false);
        }
    }
}
