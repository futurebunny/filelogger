#undef USE_INTEMPORAL_SYSTEM
#define USE_INTEMPORAL_EXPERIMENTAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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

    public static class Debugging
    {
#if USE_NATIVE_OUTPUT_DEBUG_STRING
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
#else
        static internal class NativeWin32
        {
            internal static int TotalCalls = 0;
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
                if (NativeWin32.CheckIfDebugggerAttached())
                {
                    if ((NativeWin32.AlwaysUseNativeOutput) || (count > 1))
                    {
                        NativeWin32.OutputDebugString(message);
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(message);
#else
                        System.Diagnostics.Trace.WriteLine(message);
                        //NativeWin32.OutputDebugString(message);
#endif
                    }
                }
                else
                {
                    NativeWin32.OutputDebugString(message);
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

        public static void TraceMethodEntry(bool fVerbose, string? message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Debugging.WriteLine($"TRACE: >>{memberName}() mesage: {message}");
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
                Debugging.WriteLine($"TRACE: <<{memberName}() mesage: {message}");
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
                Debugging.WriteLine($"TRACE: {memberName}() mesage: {message}");
            }

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

            if (fVerbose)
            {
                Debugging.WriteLine($"TRACE: {memberName}() source file path: {sourceFilePath}");
                Debugging.WriteLine($"TRACE: {memberName}() source line number: {sourceLineNumber}");
            }
            Debugging.WriteLine($"TRACE: <<ShowConfiguration()");
        }

    }
}
