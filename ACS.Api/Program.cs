using ACS.Admin;
using ACS.Shared.Configuration;
using ACS.Shared.Logging;
using ACS.Shared.Utilities;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using System.Security.Cryptography;
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
                                    X509Certificate2Collection chain = LoadServerCertificateFromPEM(serverConfig.Tls);
                                    X509Certificate2 cert = new(chain.Export(X509ContentType.Pkcs12));

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

        /// <summary>
        /// Imports a server certificate chain and private key from PEM
        /// </summary>
        /// <returns>PKCS12 certificate, suitable for use with Kestrel HTTPS</returns>
        private static X509Certificate2Collection LoadServerCertificateFromPEM(TlsOptions tlsOptions)
        {
            string keyContent = File.ReadAllText(tlsOptions.KeyPath);
            RSA key = RSA.Create();

            if (string.IsNullOrEmpty(tlsOptions.Password))
            {
                key.ImportFromPem(keyContent);
            }
            else
            {
                key.ImportFromEncryptedPem(keyContent, tlsOptions.Password);
            }

            X509Certificate2Collection chain = [];
            chain.ImportFromPemFile(tlsOptions.CertificatePath);

            chain[0] = chain[0].CopyWithPrivateKey(key);

            return chain;
        }
    }
}
