#define INCLUDE_ILOGGER_EXTENSION_FEATURES
#if false || INCLUDE_FOR_CURRENT_PROJECT
#if true || INCLUDE_LOW_LEVEL_DEBUG_TRACING
#undef USE_INTEMPORAL_SYSTEM
#define USE_INTEMPORAL_EXPERIMENTAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if INCLUDE_ILOGGER_EXTENSION_FEATURES
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif

#if USE_INTEMPORAL_SYSTEM
//namespace Intemporal.System.Diagnostix
namespace Intemporal.Experimental.Diagnostics.Logging
#elif USE_INTEMPORAL_EXPERIMENTAL
//namespace Intemporal.Experimental.Diagnostics.NativeMethods
#if INCLUDE_ILOGGER_EXTENSION_FEATURES
namespace Intemporal.Experimental.Diagnostics.Logging
#else
namespace Intemporal.Experimental.Diagnostics
#endif
#endif
{

    public static class Debugging
    {
#if true || ENABLE_DEBUGGING_CONFIGURATION
        // maybe make this a singleton?
        public static DebuggingConfiguration? CurrentConfig = null;
#endif

#if true || ENABLE_DEBUGGING_CONTEXT
        // maybe make this a singleton?
        public static DebuggingContext? CurrentContext = null;

#if true || ENABLE_HISTORICAL_TRACE_LISTENER
        public static System.Diagnostics.TextWriterTraceListener? LegacyTraceListener = null;
#endif

#endif

        //
        // To be used to set global configuration options for legacy tracing...
        // TODO: ==> make this work with the new .NET Core loading of options and configuration data...
        //
#if true || ENABLE_DEBUGGING_CONFIGURATION
        public struct DebuggingConfiguration
        {
            public DateTime? dtAppTimeStamp { get; set; }

            public bool? HaltIfDebuggerAttached { get; set; }

            public bool? UseCustomApplicationName { get; set; }

            public string? CustomApplicationName { get; set; }

            public Type? ApplicationType { get; set; }

            public string? DebugWritelineFormat { get; set; }

            public bool? TimestampOutput { get; set; }

            public bool? UseUtcTimezone { get; set; }

            public string? TimestampFormat { get; set; }
        }
#endif



        //
        // To be used to grab the name of a hosted service...
        //
#if true || ENABLE_DEBUGGING_CONTEXT
        public struct DebuggingContext
        {
            public DateTime? dtAppTimeStamp { get; set; }

            public bool? IsDebuggerAttached { get; set; }

            public bool? IsExecutingAssemblyValid { get; set; }

            public Type? ExecutingAssemblyType { get; set; }

            public object? ExecutingAssembly { get; set; }

            public string? ExecutingAssemblyFullName { get; set; }

            public string? ExecutingAssemblyShortName { get; set; }

            public string? ExecutingAssemblyVersion { get; set; }

            public bool? IsOwningAssemblyValid { get; set; }

            public Type? ChildItemType { get; set; }

            public object? OwningAssembly { get; set; }

            public string? OwningAssemblyFullName { get; set; }

            public string? OwningAssemblyShortName { get; set; }

            public string? OwningAssemblyVersion { get; set; }
        }
#endif



#if USE_NATIVE_OUTPUT_DEBUG_STRING
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
#else
        static internal class NativeWin32
        {
            internal static int TotalCalls = 0;
            internal static int TotalDebugCalls = 0;
            internal static int TotalTraceCalls = 0;
            internal static int TotalNativeCalls = 0;
            internal static int NoManagedListenerCalls = 0;
            internal static int RecursionCount = 0;
            internal static int MaxRecursionDepth = 10;
            internal static bool CountTotalCalls = true;
            internal static bool CheckRecursionDepth = true;
            internal static bool AlwaysUseNativeOutput = false;
            internal static bool? IsDebuggerAttached = null;



            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            internal static extern void OutputDebugString(string message);

            internal static bool CheckIfDebugggerAttached()
            {
                // Only check at startup and initialization time once...
                if (IsDebuggerAttached == null)
                {
                    IsDebuggerAttached = System.Diagnostics.Debugger.IsAttached;
                    AlwaysUseNativeOutput = !(bool)IsDebuggerAttached;
                }
                return (bool)IsDebuggerAttached;
            }
        }

        public static void WriteLine(string? message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Debugging.OutputDebugString(message);
            }
        }

        public static void OutputDebugString(string message)
        {
            int count = Interlocked.Increment(ref NativeWin32.RecursionCount);

            int totalCalls = Interlocked.Increment(ref NativeWin32.TotalCalls);

            if (count > NativeWin32.MaxRecursionDepth)
            {
#if DEBUG
                NativeWin32.OutputDebugString($"EXCEPTION: {nameof(NativeWin32)}.OutputDebugString(string?) has too many recursions={count}");
#endif
                throw new InvalidOperationException($"EXCEPTION: {nameof(NativeWin32)}.OutputDebugString(string?) has too many recursions={count}");
            }

            if (!String.IsNullOrEmpty(message))
            {
#if true || ADD_TIMESTAMP_TO_TRACE_OUTPUT
                //
                // Yuck, it's late and I just want to see if this timestamping works... but OMG this shit must generate gigabytes and beyond of GC garbage from all this string bullshit...
                //
                DateTime dtAppTimeStamp = DateTime.UtcNow;

                string szOriginalMessage = message;

                message = $"{dtAppTimeStamp.ToStringLocalInvariant()}: {szOriginalMessage}";
#endif


                //
                // Consider if this should check for any registered trace listeners... and default to Native mode if none are available...
                //
                bool? fManagedListenersAvailable = null;
#if true || CHECK_LEGACY_MANAGED_TRACE_LISTENERS
                try
                {
                    int iActiveListeners = System.Diagnostics.Trace.Listeners.Count;

                    if (iActiveListeners > 0)
                    {
                        fManagedListenersAvailable = true;
                    }
                    else
                    {
                        fManagedListenersAvailable = false;
                        int noListenerCalls = Interlocked.Increment(ref NativeWin32.NoManagedListenerCalls);
                    }
                }
                catch (Exception ex)
                {
                    //Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"EXCEPTION: Attempting to check for active legacy Trace Listeners.");
                    fManagedListenersAvailable = false;
                }
#endif

                if (NativeWin32.CheckIfDebugggerAttached())
                {
                    if ((NativeWin32.AlwaysUseNativeOutput) || (count > 1))
                    {
                        NativeWin32.OutputDebugString(message);
                        int iTotalNativeCalls = Interlocked.Increment(ref NativeWin32.TotalNativeCalls);
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(message);
                        int iTotalDebugCalls = Interlocked.Increment(ref NativeWin32.TotalDebugCalls);
#elif TRACE
                        System.Diagnostics.Trace.WriteLine(message);
                        int iTotalDebugCalls = Interlocked.Increment(ref NativeWin32.TotalTraceCalls);
#else
                        NativeWin32.OutputDebugString(message);
                        int iTotalNativeCalls = Interlocked.Increment(ref NativeWin32.TotalNativeCalls);
#endif
                    }
                }
                else
                {
                    NativeWin32.OutputDebugString(message);
                    int iTotalNativeCalls = Interlocked.Increment(ref NativeWin32.TotalNativeCalls);
                }
            }

            count = Interlocked.Decrement(ref NativeWin32.RecursionCount);


            if (count < 0)
            {
#if DEBUG
                NativeWin32.OutputDebugString($"EXCEPTION: {nameof(NativeWin32)}.OutputDebugString(string?) has too few recursions={count}");
#endif
                throw new InvalidOperationException($"EXCEPTION: {nameof(NativeWin32)}.OutputDebugString(string?) has too few recursions={count}");
            }

        }
#endif


        //
        // Consider for later that we may want to get the running service and include that in a log file name...
        //
        public static DebuggingConfiguration GetDebuggingConfiguration()
        {
            DateTime dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_CONTEXT_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Starting GetDebuggingConfiguration() at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Querying Debugging Configuration at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

            //
            // NOTE: This is not truly thread safe, but acts like a psuedo singleton... 
            //
            DebuggingConfiguration debuggingConfiguration = Debugging.CurrentConfig?? new DebuggingConfiguration();

            dtAppTimeStamp = DateTime.UtcNow;

            debuggingConfiguration.dtAppTimeStamp = dtAppTimeStamp;

#if true || CAPTURE_CONTEXT_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Finishing GetDebuggingConfiguration()  at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif
            return debuggingConfiguration;
        }

            //
            // Consider for later that we may want to get the running service and include that in a log file name...
            //
            public static DebuggingContext GetDebuggingContext(Type? _childItemType)
        {
            DateTime dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_CONTEXT_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Starting GetDebuggingContext() at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Querying for Existing Debugging Context at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

            //
            // NOTE: This is not truly thread safe, but acts like a psuedo singleton... 
            //
            DebuggingContext debuggingContext = Debugging.CurrentContext ?? new DebuggingContext();

            //
            // Update/Touch the debug context timestamp
            //
            debuggingContext.dtAppTimeStamp = dtAppTimeStamp;

            if (null == Debugging.CurrentContext)
            {
                //
                // we need to generate all the following
                //
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Initializing Debugging Context at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

                //
                // Get name of running application or hosted assembly to use when creating log files...
                //
                Assembly owningAssembly;
                Assembly executingAssembly;

                if (_childItemType != null)
                {
                    debuggingContext.ChildItemType = _childItemType;
                    owningAssembly = _childItemType.Assembly;
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Type [{_childItemType.ToString()}] is owned/contained in assembly [{owningAssembly.FullName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

                    //
                    // Save this or null out
                    //
                    debuggingContext.OwningAssembly = owningAssembly;
                    debuggingContext.OwningAssemblyFullName = owningAssembly.FullName;
                    AssemblyName owningAssemblyName = owningAssembly.GetName();
                    debuggingContext.OwningAssemblyShortName = owningAssemblyName.Name;
                    if (null != owningAssemblyName.Version)
                    {
                        debuggingContext.OwningAssemblyVersion = owningAssemblyName.Version.ToString();
                    }
                    else
                    {
                        debuggingContext.OwningAssemblyVersion = null;
                    }
                    debuggingContext.IsOwningAssemblyValid = true;
                }
                else
                {
                    debuggingContext.IsOwningAssemblyValid = false;
                    debuggingContext.OwningAssembly = null;
                    debuggingContext.OwningAssemblyFullName = null;
                    debuggingContext.OwningAssemblyShortName = null;
                    debuggingContext.OwningAssemblyVersion = null;
                }

                //
                // Get the currently executing assembly... can this ever throw an exception or return null... if so, this whole thing should be wrapped...
                //
                executingAssembly = Assembly.GetExecutingAssembly();

                if (null != executingAssembly)
                {
                    //
                    // Save this or null out
                    //
                    debuggingContext.ExecutingAssembly = executingAssembly;
                    debuggingContext.ExecutingAssemblyFullName = executingAssembly.FullName;
                    AssemblyName executingAssemblyName = executingAssembly.GetName();
                    debuggingContext.ExecutingAssemblyShortName = executingAssemblyName.Name;
                    if (null != executingAssemblyName.Version)
                    {
                        debuggingContext.ExecutingAssemblyVersion = executingAssemblyName.Version.ToString();
                    }
                    else
                    {
                        debuggingContext.ExecutingAssemblyVersion = null;
                    }
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Currently Executing Assembly is [{executingAssembly.FullName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
                    debuggingContext.IsExecutingAssemblyValid = true;
                }
                else
                {
                    debuggingContext.IsExecutingAssemblyValid = false;
                    debuggingContext.ExecutingAssembly = null;
                    debuggingContext.ExecutingAssemblyFullName = null;
                    debuggingContext.ExecutingAssemblyShortName = null;
                    debuggingContext.ExecutingAssemblyVersion = null;
                }

                if (null == Debugging.CurrentContext)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Saving Debugging Context at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
                    Debugging.CurrentContext = debuggingContext;
                }
            }
            else
            {
                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"DEBUG: Returning Previously Initialized Debugging Context at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
            }


#if true || CAPTURE_CONTEXT_CREATION
            dtAppTimeStamp = DateTime.UtcNow;

            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Finishing GetDebuggingContext()  at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            return debuggingContext;
        }

        //
        // Add method to create and save trace listener output... also have to check if this needs to happen both before creating the ILogger adapter and afterwards...
        //
        public static void TraceCreateLogFileWriter(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            DateTime dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_WORKER_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Starting TraceCreateLogFileWriter()  at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            bool fLogFileNameCreated = false;
#if true || TEST_CODE_FOR_LOG_FILE_NAME
            //
            // Generate Log File Name: e.g. "RegistryServerApp--dt2022-04-02--00-22-20--TickCount[8DA143ED9C9F04E]--TraceListener.log.txt"
            //
            //
            string szLogFileDate = "2022-04-02";
            string szLogFileTime = "00-22-20";
            string szTickCount = dtAppTimeStamp.ToStringHexFullLength();
            string szCustomAppName = "RegistryServerApp";
            string szAppName = szCustomAppName;
            string szTraceFileCustomAppName    = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
            string szTraceFileOwningAppName    = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
            string szTraceFileExecutingAppName = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
            string szTraceFileName = String.Empty;
            string szTraceFileFullPath = $".\\Logs\\{szTraceFileName}";

            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Example => TraceWriter target filename would be [{szTraceFileCustomAppName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Example => TraceWriter target filename would be [{szTraceFileOwningAppName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
            Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Example => TraceWriter target filename would be [{szTraceFileExecutingAppName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

#if true || QUERY_CONFIGURATION_FOR_LOG_FILE_NAME
            //
            // Note, this should return a new or existing configuration object...
            //
            Intemporal.Experimental.Diagnostics.Logging.Debugging.DebuggingConfiguration _DBGConfig = Intemporal.Experimental.Diagnostics.Logging.Debugging.GetDebuggingConfiguration();

            bool fUseCustomName = _DBGConfig.UseCustomApplicationName ?? false;

            if (fUseCustomName)
            {
                szCustomAppName = _DBGConfig.CustomApplicationName ?? String.Empty;

                //
                // For now, always create date and time values that are relative to local (file) system...
                //
                if (szCustomAppName != String.Empty)
                {
                    dtAppTimeStamp = _DBGConfig.dtAppTimeStamp ?? DateTime.UtcNow;
                    dtAppTimeStamp = dtAppTimeStamp.ToLocalTime();
                    szLogFileDate = dtAppTimeStamp.ToString("yyyy-MM-dd"); // "2022-04-02";
                    szLogFileTime = dtAppTimeStamp.ToString("HH-mm-ss"); // "00-22-20";
                    szTickCount = dtAppTimeStamp.ToStringHexFullLength(); // "8DA143ED9C9F04E"
                    szAppName = szCustomAppName;

                    szTraceFileCustomAppName = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
                    
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Example => TraceWriter target filename would be [{szTraceFileCustomAppName}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

                    szTraceFileName = szTraceFileCustomAppName;

                    fLogFileNameCreated = true;
                }
            }
            else
            {
                szCustomAppName = String.Empty;
                szTraceFileName = String.Empty;
            }

#endif


#if true || CAPTURE_APPLICATION_EXECUTABLE_CONTEXT
            if (String.IsNullOrEmpty(szTraceFileName) || !fLogFileNameCreated)
            {
                //
                // Note, this is fucking this up, don't re run this code if we already have a context object...
                //
                Intemporal.Experimental.Diagnostics.Logging.Debugging.DebuggingContext _DC = Intemporal.Experimental.Diagnostics.Logging.Debugging.GetDebuggingContext(null);

                //("yyyy-MM-dd--HH:mm:ss.fffffffZzzz");
                //"DateFormat": "yyyy-MM-dd",
                //"DateFormat": "yyyy-MM-dd--HH:mm:ss.fffffffZzzz",
                //szAppName = _DC.ExecutingAssemblyShortName; // "RegistryServerApp";

                //
                // For now, always create date and time values that are relative to local (file) system...
                //
                dtAppTimeStamp = _DC.dtAppTimeStamp ?? DateTime.UtcNow;
                dtAppTimeStamp = dtAppTimeStamp.ToLocalTime();
                szLogFileDate = dtAppTimeStamp.ToString("yyyy-MM-dd"); // "2022-04-02";
                szLogFileTime = dtAppTimeStamp.ToString("HH-mm-ss"); // "00-22-20";
                szTickCount = dtAppTimeStamp.ToStringHexFullLength(); // "8DA143ED9C9F04E"

                bool fUseOwningAssemblyName = _DC.IsOwningAssemblyValid ?? false;

                if (fUseOwningAssemblyName)
                {
                    szAppName = _DC.OwningAssemblyShortName ?? "UnknownApplication";
                    szTraceFileOwningAppName = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
                    szTraceFileName = szTraceFileOwningAppName;
                    fLogFileNameCreated = true;
                }
                else if (_DC.IsExecutingAssemblyValid?? false)
                {
                    szAppName = _DC.ExecutingAssemblyShortName ?? "UnknownApplication";
                    szTraceFileExecutingAppName = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
                    szTraceFileName = szTraceFileExecutingAppName;
                    fLogFileNameCreated = true;
                }
                else
                {
                    szAppName = "UnknownApplication";
                    szTraceFileName = $"{szAppName}--dt{szLogFileDate}--{szLogFileTime}--TickCount[{szTickCount}]--TraceListener.log.txt";
                    fLogFileNameCreated = true;
                }
           }
#endif
            //
            // Make sure we have a real name to use for the path... DOH!! should test if it exists and/or we can create/flatten/append...
            //
            if (fLogFileNameCreated && !String.IsNullOrEmpty(szTraceFileName))
            {
                szTraceFileFullPath = $".\\Logs\\{szTraceFileName}";

                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Dynamic TraceWriter target filename would be [{szTraceFileFullPath}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

                //
                // Create Legacy Trace Listener and Consider holding on to this internally...and also returning it... will explain later...
                //
                //System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener("TextWriterOutput.log", "myListener"));
                System.Diagnostics.TextWriterTraceListener twTraceListener = new TextWriterTraceListener(szTraceFileFullPath, "legacyAppListener");

                Intemporal.Experimental.Diagnostics.Logging.Debugging.LegacyTraceListener = twTraceListener;

                Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"INFORMATION: Created Legacy TraceWriter target filename would be [{szTraceFileFullPath}] at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
            }


            dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_CONTEXT_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Finishing TraceCreateLogFileWriter()  at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            return;
        }

        public static void SaveTraceToLogFile(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            DateTime dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_WORKER_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodEntry(false, $">> Starting SaveTraceToLogFile() at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

            if (!String.IsNullOrEmpty(message))
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() message: {message}");
            }
            else
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() message: Now Adding Legacy Trace Lister to save output to disk / file");
            }

            if (fVerbose)
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() source file path: {sourceFilePath}");
                Debugging.WriteLine($"TRACE: >>{memberName}() source line number: {sourceLineNumber}");
            }

            //
            // This last step actually clears the existing trace listeners and just adds the file writer version... lol, i think it would have been easier to use this all along
            //
            bool s_fAddTraceListerToLogger = (Intemporal.Experimental.Diagnostics.Logging.Debugging.LegacyTraceListener != null);
            System.Diagnostics.TextWriterTraceListener twTraceListener;

#if true || SAVE_LEGACY_TRACE_OUTPUT_TO_FILE

            if (Intemporal.Experimental.Diagnostics.Logging.Debugging.LegacyTraceListener != null)
            {
                //
                // Get a reference to the global legacy listener...
                //

                twTraceListener = Intemporal.Experimental.Diagnostics.Logging.Debugging.LegacyTraceListener;
                Debugging.WriteLine($"CRITICAL: Successfully created legacy TraceListener to capture legacy output to a file...");

                if (s_fAddTraceListerToLogger)
                {
                    Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"IMPORTANT: Preparing to clear and add new file writer trace listener to the empty collection at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");

                    int iActiveListeners = System.Diagnostics.Trace.Listeners.Count;

                    if (iActiveListeners > 0)
                    {
                        Debugging.WriteLine($"WARNING: Clearing any legacy Trace Listeners to avoid recursive loops in output...");
                        System.Diagnostics.Trace.Listeners.Clear();
                        iActiveListeners = System.Diagnostics.Trace.Listeners.Count;
                    }


                    int iListenerIndex = System.Diagnostics.Trace.Listeners.Add(twTraceListener);

                    int iNewActiveListeners = System.Diagnostics.Trace.Listeners.Count;

                    if (iNewActiveListeners > iActiveListeners)
                    {
                        Debugging.WriteLine($"IMPORTANT: Successfully added new trace listener to the current collection");
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"IMPORTANT: Successfully added new trace listener to the current collection at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
                    }
                    else
                    {
                        Debugging.WriteLine($"ERROR: Failed to add new trace listener to the current collection");
                        Intemporal.Experimental.Diagnostics.Logging.Debugging.WriteLine($"CRITICAL: Failed to add new trace listener to the current collection at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
                    }
                }
            }
#endif



            dtAppTimeStamp = DateTime.UtcNow;

#if true || CAPTURE_CONTEXT_CREATION
            Intemporal.Experimental.Diagnostics.Logging.Debugging.TraceMethodExit(false, $"<< Finishing SaveTraceToLogFile() at: {dtAppTimeStamp.ToStringLocalInvariant()}, equal to [0x{dtAppTimeStamp.ToStringHexFullLength()}]");
#endif

        }

        public static void TraceMethodEntry(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() message: {message}");
            }
            else
            {
                Debugging.WriteLine($"TRACE: >>{memberName}()");
            }

            if (fVerbose)
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() source file path: {sourceFilePath}");
                Debugging.WriteLine($"TRACE: >>{memberName}() source line number: {sourceLineNumber}");
            }
        }

        public static void TraceMethodExit(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Debugging.WriteLine($"TRACE: <<{memberName}() message: {message}");
            }
            else
            {
                Debugging.WriteLine($"TRACE: <<{memberName}()");
            }
            if (fVerbose)
            {
                Debugging.WriteLine($"TRACE: <<{memberName}() source file path: {sourceFilePath}");
                Debugging.WriteLine($"TRACE: <<{memberName}() source line number: {sourceLineNumber}");
            }
        }


        public static void ShowConfiguration(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Debugging.WriteLine($"TRACE: >>ShowConfiguration()");

            Debugging.WriteLine($"TRACE: >>{memberName}()");
            if (String.IsNullOrEmpty(message))
            {
                Debugging.WriteLine($"TRACE: {memberName}() message: {message}");
            }


            //
            // TODO - decide if all of this should be in that bulk debugging context??
            //
#if DEBUG
            NativeWin32.OutputDebugString($"TRACE: {nameof(Debugging)}.OutputDebugString(string?) has been called [{NativeWin32.TotalCalls}] times thus far");
#endif

            //internal static int TotalCalls = 0;
            //internal static int RecursionCount = 0;
            //internal static int MaxRecursionDepth = 10;
            //internal static bool CountTotalCalls = true;
            //internal static bool CheckRecursionDepth = true;
            //internal static bool AlwaysUseNativeOutput = true;
            //internal static bool? IsDebuggerAttached = null;

            Debugging.WriteLine($"INFORMATION: CONFIGURATION CheckIfDebugggerAttached =[{NativeWin32.CheckIfDebugggerAttached}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION IsDebuggerAttached       =[{NativeWin32.IsDebuggerAttached}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION CountTotalCalls          =[{NativeWin32.CountTotalCalls}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION CheckRecursionDepth      =[{NativeWin32.CheckRecursionDepth}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION AlwaysUseNativeOutput    =[{NativeWin32.AlwaysUseNativeOutput}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION MaxRecursionDepth        =[{NativeWin32.MaxRecursionDepth}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION TotalCalls               =[{NativeWin32.TotalCalls}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION TotalDebugCalls          =[{NativeWin32.TotalDebugCalls}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION TotalTraceCalls          =[{NativeWin32.TotalTraceCalls}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION TotalNativeCalls         =[{NativeWin32.TotalNativeCalls}]");
            Debugging.WriteLine($"INFORMATION: CONFIGURATION NoManagedListenerCalls   =[{NativeWin32.NoManagedListenerCalls}]");

            if (fVerbose)
            {
                Debugging.WriteLine($"TRACE: {memberName}() source file path: {sourceFilePath}");
                Debugging.WriteLine($"TRACE: {memberName}() source line number: {sourceLineNumber}");
            }
            Debugging.WriteLine($"TRACE: <<ShowConfiguration()");
        }

    }
}

#else
#undef USE_INTEMPORAL_SYSTEM
#define USE_INTEMPORAL_EXPERIMENTAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if USE_INTEMPORAL_SYSTEM
//namespace Intemporal.System.Diagnostix
namespace Intemporal.Experimental.Diagnostics.Logging
#elif USE_INTEMPORAL_EXPERIMENTAL
namespace Intemporal.Experimental.Diagnostics.NativeMethods
#endif
{
    public class NativeWin32
    {
#if true || USE_NATIVE
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
#else
        private static int m_totalCalls = 0;
        private static int m_recursionCount = 0;

        public static void OutputDebugString(string message)
        {
            int count = Interlocked.Increment(ref m_recursionCount);

            int totalCalls = Interlocked.Increment(ref m_totalCalls);

            if (count > 10)
            {
                //Intemporal.Experimental.Diagnostics.NativeMethods.NativeWin32.OutputDebugString($"TRACE: {nameof(TraceListenerToLogger)}.WriteLine(string?) with Instance {iHashCode} has {count} recursions");
                throw new InvalidOperationException($"TRACE: {nameof(NativeWin32)}.OutputDebugString(string?) has too many recursions={count}");
            }

            System.Diagnostics.Trace.WriteLine(message);

            count = Interlocked.Decrement(ref m_recursionCount);

            //Intemporal.Experimental.Diagnostics.NativeMethods.NativeWin32.OutputDebugString($"TRACE: {nameof(TraceListenerToLogger)}.WriteLine(string?) with Instance {iHashCode} has {count} recursions");

            if (count < 0)
            {
                //Intemporal.Experimental.Diagnostics.NativeMethods.NativeWin32.OutputDebugString($"TRACE: {nameof(TraceListenerToLogger)}.WriteLine(string?) with Instance {iHashCode} has {count} recursions");
                throw new InvalidOperationException($"TRACE: {nameof(NativeWin32)}.OutputDebugString(string?) has too few recursions={count}");
            }

        }
#endif

    }

    //namespace OutputDebugString
    //{
    //    class Program
    //    {

    //        static void Main(string[] args)
    //        {
    //            Console.WriteLine("Main - Enter - Console.WriteLine");
    //            Debug.WriteLine("Main - Enter - Debug.WriteLine");
    //            OutputDebugString("Main - Enter - OutputDebugString");
    //            OutputDebugString("Main - Exit - OutputDebugString");
    //            Debug.WriteLine("Main - Exit - Debug.WriteLine");
    //            Console.WriteLine("Main - Exit - Console.WriteLine");
    //        }
    //    }
    //}

}
#endif
#else
namespace Intemporal.Experimental.Diagnostics.Logging
{
    public static class Debugging
    {
        public static void WriteLine(string? message)
        {
        }
        public static void TraceMethodEntry(bool fVerbose, string? message)
        {
        }
        public static void TraceMethodExit(bool fVerbose, string? message)
        {
        }

    }
}
#endif
