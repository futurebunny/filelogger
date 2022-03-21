#define DEBUG_INC_TIMESTAMPED_LOGS
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomFormat
{
    // This sample demonstrates how to customize the output of the logger by
    // subclassing FileLogEntryTextBuilder and overriding its virtual methods.
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            bool fDelayBeforeExit = true;
            //
            //
            //
            System.Diagnostics.Debug.WriteLine($"WARNING: No parameters were provided on command line, using static ones as default...");
            System.Console.WriteLine($"WARNING: No parameters were provided on command line, using static ones as default...");


#if DEBUG
#if DEBUG_INC_TIMESTAMPED_LOGS
                var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.Timestamped.json")
                .Build();
#else
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json")
                .Build();
#endif
#else
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
#endif

#if SPRINT_4
            string? szLogFileName = null;

            if (null != configuration)
            {
                IConfigurationSection iLoggingConfiguration = null;
                IConfigurationSection iFileLoggerConfiguration = null;

                //IConfigurationSection iFileLoggerConfiguration = configuration.GetRequiredSection("File");

                //Key "Logging:File:Files:0:Path" string

                iLoggingConfiguration = configuration.GetSection("Logging");

                iFileLoggerConfiguration = iLoggingConfiguration.GetSection("File");

                //Key "Logging:File:Files:1:Path" string
                //iFileLoggerConfiguration.GetSection("Files");

                if (iFileLoggerConfiguration != null)
                {
                    object oPath = iFileLoggerConfiguration.GetValue(szLogFileName.GetType(), "Path");

                    object oLogFileName = iFileLoggerConfiguration.GetValue(typeof(string), "Path");

                    if ((oPath != null) && (oPath.GetType() == typeof(string)))
                    {
                        szLogFileName = oPath.ToString();
                    } 
                    else if ((oLogFileName != null) && (oLogFileName.GetType() == typeof(string)))
                    {
                        szLogFileName = oLogFileName.ToString();
                    }
                }
            }
            if (String.IsNullOrEmpty(szLogFileName))
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: PathNameParameter is: {szLogFileName}");
                System.Console.WriteLine($"DEBUG: PathNameParameter is: {szLogFileName}");
            }
#endif
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));

                builder.AddFile(configure: o =>
                {
                    o.RootPath = AppContext.BaseDirectory;

                    // This is how the custom text builder is configured by code,
                    // but in this case we do that in appsettings.json.
                    //o.TextBuilder = SingleLineLogEntryTextBuilder.Default;
                });
            });

            await using (ServiceProvider sp = services.BuildServiceProvider())
            {
                // create logger
                var logger = sp.GetRequiredService<ILogger<Program>>();

                logger.LogWarning($"This should always be the first message logged upon starting this app at TickCount={DateTime.UtcNow.Ticks}");

                logger.LogInformation("A non-scoped message.");
                using (logger.BeginScope("1st level scope"))
                {
                    logger.LogInformation("A 1st scoped message.");

                    logger.LogInformation("A 2nd scoped message.");

                    logger.LogWarning(" About to call a method which could result in an exception, watch for log output");

                    using (logger.BeginScope("A nested 2nd level scope"))
                    {
                        try {
                            logger.LogError($"Target test method generated an error which will result in an exception, watch for corresponding log output");
                            throw new ApplicationException("Some error."); 
                        }
                        catch (Exception ex) 
                        {
                            logger.LogError($"Caught a handled exception, logging it as a critical message");
                            logger.LogCritical(ex, $"Another scoped multi-line message {Environment.NewLine} with exception.");
                            logger.LogError(ex, $"Another scoped multi-line message {Environment.NewLine} with exception.");
                        }
                    }

                    logger.LogWarning($"All operations should be complete and no output should be logged after this message at TickCount={DateTime.UtcNow.Ticks}");

                }
            }

            //
            // finished await of using service provider....
            //
            System.Diagnostics.Debug.WriteLine($"WARNING: Preparing to exit main thread/task");
            System.Console.WriteLine($"WARNING: Preparing to exit main thread/task");

            //
            //
            //
            if (fDelayBeforeExit)
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: Pausing for 5sec to ensure debugging is complete");
                System.Console.WriteLine($"WARNING: Pausing for 5sec to ensure debugging is complete");
                await Task.Delay(5000);
            }
         

            System.Diagnostics.Debug.WriteLine($"CRITICAL: Exiting main thread/task");
            System.Console.WriteLine($"CRITICAL: Exiting main thread/task");
        }
    }
}
