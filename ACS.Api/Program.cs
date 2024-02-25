using ACS.Admin;
using ACS.Shared.Configuration;
using ACS.Shared.Logging;
using Serilog;

namespace ACS.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel((context, kestrelOptions) =>
                        {
                            IConfiguration appConfig = context.Configuration.GetRequiredSection(Startup.ConfigurationRoot);
                            ServerConfiguration? serverConfiguration = appConfig.GetRequiredSection("Server").Get<ServerConfiguration>();

                            if (serverConfiguration != null)
                            {
                                kestrelOptions.ListenAnyIP(serverConfiguration.Port, listenOptions =>
                                {
                                    listenOptions.UseHttps();
                                    /*listenOptions.UseHttps(X509Certificate2.CreateFromEncryptedPemFile(
                                        serverConfiguration.Tls.CertificatePath,
                                        serverConfiguration.Tls.Password,
                                        serverConfiguration.Tls.KeyPath));*/
                                });
                            }
                            else
                            {
                                throw new ArgumentException($"Missing server configuration");
                            }
                        });
                })
                .UseSerilog(LoggingUtils.ConfigureLogging)
                .Build()
                .Run();
        }
    }
}
