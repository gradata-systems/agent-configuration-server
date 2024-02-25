using AgentConfigurationServer.Configuration;
using AgentConfigurationServer.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.ResponseCompression;
using NuGet.Packaging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace AgentConfigurationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>();

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                ConfigureLogging(hostingContext, loggerConfiguration);
            });

            ConfigureAuthentication(builder);
            ConfigureAuthorisation(builder);

            // Response compression
            builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            builder.Services.AddResponseCompression();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "admin",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ConfigureLogging(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithApiContext();    // Custom enricher, adding user attribution and remote host information

            LoggingConfiguration appLoggingConfig = hostingContext.Configuration.GetSection("Logging").Get<LoggingConfiguration>();

            try
            {
                if (appLoggingConfig.Console.Enabled)
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
                    if (httpConfig.Persistence.Enabled)
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
                            queueLimitBytes: null,
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

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            OpenIdConfiguration? config = builder.Configuration.GetSection("OpenId").Get<OpenIdConfiguration>();
            if (config != null)
            {
                builder.Services
                    .AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                    .AddCookie()
                    .AddOpenIdConnect(options =>
                    {
                        options.Authority = config.AuthorityUrl;
                        options.ClientId = config.ClientId;
                        options.ClientSecret = config.ClientSecret;
                        options.ResponseType = "code";
                        options.Scope.AddRange(config.Scopes);
                        options.SaveTokens = true;
                        options.UsePkce = true;
                    });
            }
        }

        private static void ConfigureAuthorisation(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
