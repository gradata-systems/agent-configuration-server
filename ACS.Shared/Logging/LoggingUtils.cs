using ACS.Shared.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Configuration;
using Serilog.Formatting.Json;

namespace ACS.Shared.Logging
{
    public static class LoggingUtils
    {
        public static void ConfigureLogging(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithApiContext();

            LoggingConfiguration appLoggingConfig = hostingContext.Configuration.GetRequiredSection("Logging").Get<LoggingConfiguration>();

            try
            {
                if (appLoggingConfig != null && appLoggingConfig.Console.Enabled)
                {
                    loggerConfiguration.WriteTo.Console(new JsonFormatter());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid console logging configuration", ex);
            }

            try
            {
                HttpLoggingConfiguration httpConfig = appLoggingConfig.Http;

                if (httpConfig.Enabled && !string.IsNullOrEmpty(httpConfig.Url))
                {
                    if (httpConfig.Persistence != null && httpConfig.Persistence.Enabled)
                    {
                        if (!IsDirectoryWriteable(httpConfig.Persistence.Directory))
                        {
                            throw new Exception($"Log directory {httpConfig.Persistence.Directory} is not writeable");
                        }

                        loggerConfiguration.WriteTo.DurableHttpUsingTimeRolledBuffers(
                            httpClient: new HttpLoggingClient(),
                            configuration: hostingContext.Configuration,
                            requestUri: httpConfig.Url,
                            restrictedToMinimumLevel: httpConfig.MinimumLogLevel ?? LogEventLevel.Information,
                            bufferBaseFileName: Path.Combine(httpConfig.Persistence.Directory, "log"),
                            logEventsInBatchLimit: httpConfig.BatchLimit,
                            period: httpConfig.Period,
                            bufferRollingInterval: Serilog.Sinks.Http.BufferRollingInterval.Minute
                        );
                    }
                    else
                    {
                        loggerConfiguration.WriteTo.Http(
                            httpClient: new HttpLoggingClient(),
                            configuration: hostingContext.Configuration,
                            requestUri: httpConfig.Url,
                            restrictedToMinimumLevel: httpConfig.MinimumLogLevel ?? LogEventLevel.Information,
                            logEventsInBatchLimit: httpConfig.BatchLimit,
                            queueLimitBytes: httpConfig.InMemoryQueueLimitBytes,
                            period: httpConfig.Period
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid HTTP logging configuration", ex);
            }
        }

        /// <summary>
        /// Determines whether the running process has file creation rights within a directory
        /// </summary>
        /// <param name="path">Path to the directory to test</param>
        private static bool IsDirectoryWriteable(string path)
        {
            try
            {
                using FileStream fs = File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
