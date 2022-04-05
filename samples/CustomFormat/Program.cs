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
            try
            {

#if DEBUG
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json")
                .Build();
#else
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
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
                            try
                            {
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
   
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Application hit an unhandled exception");
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                if (null != ex.StackTrace)
                {
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace.ToString());
                }
            }
        
        }  // async Task Main

    }
}
