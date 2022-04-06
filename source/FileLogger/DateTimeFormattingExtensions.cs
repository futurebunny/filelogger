//
// Only one of the namespace entries should be enabled
//
//#define USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS
//#define USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS_LOGGING
//#define USE_NAMESPACE_SYSTEM_DIAGNOSTICS
//#define USE_NAMESPACE_SYSTEM_DIAGNOSTICS_LOGGING
#define USE_NAMESPACE_KARAMBOLO

//
// Use the namespace and project to select certain features and options
//
#if USE_NAMESPACE_KARAMBOLO
//#define INCLUDE_FOR_CURRENT_PROJECT
#elif (USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS || USE_NAMESPACE_SYSTEM_DIAGNOSTICS)
#define INCLUDE_FOR_CURRENT_PROJECT
#elif (USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS_LOGGING || USE_NAMESPACE_SYSTEM_DIAGNOSTICS_LOGGING)
#define INCLUDE_FOR_CURRENT_PROJECT
#define INCLUDE_ILOGGER_EXTENSION_FEATURES
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if INCLUDE_ILOGGER_EXTENSION_FEATURES
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif


#if USE_NAMESPACE_KARAMBOLO
namespace Karambolo.Extensions.Logging.File
#elif USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS
namespace Intemporal.Experimental.Diagnostics
#elif USE_NAMESPACE_SYSTEM_DIAGNOSTICS
namespace Intemporal.System.Diagnostics
#elif USE_NAMESPACE_EXPERIMENTAL_DIAGNOSTICS_LOGGING
namespace Intemporal.Experimental.Diagnostics.Logging
#elif USE_NAMESPACE_SYSTEM_DIAGNOSTICS_LOGGING
namespace Intemporal.System.Diagnostics
#endif
{
    public static class DateTimeFormattingExtensions
    {
        public static readonly string s_FormatUtcInvariant = "o";

        /// <summary>
        ///     A bool extension method that execute an Action if the value is true.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action to execute.</param>
        public static void IfEnabled(this bool @this, Action action)
        {
            if (@this)
            {
                action();
            }
        }


        /// <summary>
        ///     A bool extension method that execute an Action if the value is true.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="action">The action to execute.</param>
        //public static string ToStringUtcInvariant(this bool @this, Action action)
        //{
        //    if (@this)
        //    {
        //        action();
        //    }
        //}


        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringHexFullLength(this DateTime? @this)
        {
#if DEBUG
            bool fExceptionsHit = true;
#endif
            String szValue = string.Empty;
            try
            {
                if (@this != null)
                {
                    //
                    // Call the real method?
                    //
                    szValue = ((DateTime)@this).Ticks.ToString("X8");
                    fExceptionsHit = false;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                fExceptionsHit = true;

#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Debugging.WriteLine($"EXCEPTION: {ex.Message}");
#else
                Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Debugging.WriteLine($"EXCEPTION: {ex.Message}");
#endif
                if (null != ex.StackTrace)
                {
#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                    Debugging.WriteLine(ex.StackTrace.ToString());
#else
                    Debugging.WriteLine(ex.StackTrace.ToString());
#endif
                }
#endif
            }
            finally
            {
#if DEBUG
                if (fExceptionsHit)
                {
#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                    Debugging.TraceMethodExit(fExceptionsHit, $"Extension Method - ToStringHexFullLength() - unable to safely generate hex string from Ticks");
#else
                    Debugging.TraceMethodExit(fExceptionsHit, $"Extension Method - ToStringHexFullLength() - unable to safely generate hex string from Ticks");
#endif
                }
#endif
            }
            return szValue;
        }

        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringHexFullLength(this DateTime @this)
        {
#if DEBUG
            bool fExceptionsHit = true;
#endif
            String szValue = string.Empty;
            try
            {
                szValue = @this.Ticks.ToString("X8");
                fExceptionsHit = false;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                fExceptionsHit = true;

#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Debugging.WriteLine($"EXCEPTION: {ex.Message}");
#else
                Debugging.WriteLine($"ERROR: Formatting request resulted in a normally unhandled exception");
                Debugging.WriteLine($"EXCEPTION: {ex.Message}");
#endif
                if (null != ex.StackTrace)
                {
#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                    Debugging.WriteLine(ex.StackTrace.ToString());
#else
                    Debugging.WriteLine(ex.StackTrace.ToString());
#endif
                }
#endif
            }
            finally
            {
#if DEBUG
                if (fExceptionsHit)
                {
#if INCLUDE_ILOGGER_EXTENSION_FEATURES
                    Debugging.TraceMethodExit(fExceptionsHit, $"Extension Method - ToStringHexFullLength() - unable to safely generate hex string from Ticks");
#else
                    Debugging.TraceMethodExit(fExceptionsHit, $"Extension Method - ToStringHexFullLength() - unable to safely generate hex string from Ticks");
#endif
                }
#endif
            }
            return szValue;
        }

        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringHexFullLength(this long @this)
        {
            //
            // Hmm... will never get a Null DateTime according to compiler...
            //
            return @this.ToString("X8");
        }

        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringUtcInvariant(this DateTime @this)
        {
            //
            // Hmm... will never get a Null DateTime according to compiler...
            //
            return @this.ToString(s_FormatUtcInvariant);    
        }

        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringUtcInvariant(this DateTime? @this)
        {
            //
            // Hmm... will never get a Null DateTime according to compiler...
            //
            if (@this == null)
            {
                return String.Empty;
            }
            else
            {
                //@this?? @this.ToString(s_FormatUtcInvariant) : String.Empty;
                String szValue = ((DateTime)@this).ToString(s_FormatUtcInvariant);
                return szValue;
            }
        }


        /// <summary>
        ///     A DateTime extension method that returns a UTC invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringLocalInvariant(this DateTime @this)
        {
            //
            // Hmm... will never get a Null DateTime according to compiler...
            //
            return @this.ToLocalTime().ToString(s_FormatUtcInvariant);
        }

        /// <summary>
        ///     A DateTime extension method that returns a DateTime.ToLocalTime which is culture invariant string for the specified datetime value...using .ToString("o");
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        public static string ToStringLocalInvariant(this DateTime? @this)
        {
            if (@this != null)
            {
                return @this.ToStringLocalInvariant();
            }
            else
            {
                return String.Empty;
            }
        }

    }
}
