using ACS.Shared.Configuration;
using ACS.Shared.Logging;
using ACS.Shared.Utilities;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using System.Security.Cryptography.X509Certificates;

namespace ACS.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel((context, kestrelOptions) =>
                        {
                            IConfiguration appConfig = context.Configuration.GetRequiredSection(Startup.ConfigurationRoot);
                            ServerConfiguration? serverConfig = appConfig.GetRequiredSection("Server").Get<ServerConfiguration>();

                            if (serverConfig != null)
                            {
                                int listenPort = int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS") ?? "8443");
                                kestrelOptions.ListenAnyIP(listenPort, listenOptions =>
                                {
                                    X509Certificate2Collection chain = TlsUtils.LoadServerCertificateFromPEM(serverConfig.Tls);
                                    X509Certificate2 cert = X509CertificateLoader.LoadPkcs12(chain.Export(X509ContentType.Pkcs12), null);

                                    listenOptions.UseHttps(cert, httpsOptions =>
                                    {
                                        httpsOptions.ServerCertificateChain = chain;
                                        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;

                                        // Verification is performed by the authentication middleware
                                        httpsOptions.AllowAnyClientCertificate();
                                    });
                                });
                            }
                            else
                            {
                                throw new ArgumentException($"Missing server configuration");
                            }
                        });
                })
                .UseSerilog(LoggingUtils.ConfigureLogging)
                .Build();

            // Apply database migrations
            DbUtils.ApplyMigrations(host);

            host.Run();
        }
    }
}
