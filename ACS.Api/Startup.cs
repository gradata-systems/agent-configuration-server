using ACS.Api.Configuration;
using ACS.Shared;
using ACS.Shared.Configuration;
using ACS.Shared.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;

namespace ACS.Api
{
    public class Startup
    {
        public static readonly string ConfigurationRoot = "Api";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDbContext>();

            // Services
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITargetMatchingService, TargetMatchingService>();

            ConfigureAuthentication(services);
            ConfigureAuthorisation(services);

            // Response compression
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Add services to the container.
            services.AddControllers();

            // Bind app settings to make them available via dependency injection
            services.AddOptions();
            services.Configure<DataSourceConfiguration>(Configuration.GetSection("DataSource"));
            services.Configure<CacheConfiguration>(Configuration.GetSection("Cache"));
            services.Configure<ApiConfiguration>(Configuration.GetSection(ConfigurationRoot));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ApiAuthConfiguration authConfig = Configuration.GetRequiredSection(ConfigurationRoot)
                .GetRequiredSection("Authentication").Get<ApiAuthConfiguration>()!;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            if (!string.IsNullOrEmpty(authConfig.ForwardedHeader))
            {
                app.UseCertificateForwarding();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable gzip response compression
            app.UseResponseCompression();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "api",
                    pattern: "{controller}/{action}/{id?}"
                );
            });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            ApiAuthConfiguration authConfig = Configuration.GetRequiredSection(ConfigurationRoot)
                .GetRequiredSection("Authentication").Get<ApiAuthConfiguration>()!;

            if (!string.IsNullOrEmpty(authConfig.ForwardedHeader))
            {
                services.AddCertificateForwarding(options =>
                {
                    options.CertificateHeader = authConfig.ForwardedHeader;

                    // Load the certificate from PEM format, instead of decoding from base64
                    options.HeaderConverter = header =>
                    {
                        try
                        {
                            return X509Certificate2.CreateFromPem(HttpUtility.UrlDecode(header));
                        }
                        catch (CryptographicException ex)
                        {
                            Log.Error("Invalid client certificate {Certificate} provided in header", header, ex);
                            return null;
                        }
                    };
                });
            }

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    if (!string.IsNullOrEmpty(authConfig.CaTrustPath))
                    {
                        X509Certificate2Collection caCerts = [];
                        caCerts.ImportFromPemFile(authConfig.CaTrustPath);

                        options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
                        options.CustomTrustStore = caCerts;
                    }

                    HashSet<Regex>? subjectPatterns = authConfig.AuthorisedSubjects != null ?
                        new(authConfig.AuthorisedSubjects.Select(pattern => new Regex(pattern, RegexOptions.Compiled))) : null;

                    options.RevocationMode = X509RevocationMode.NoCheck;

                    // Disable expiry checking if configured
                    if (authConfig.ValidateCertificateExpiry == false)
                    {
                        options.ValidateValidityPeriod = false;
                    }

                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnCertificateValidated = context =>
                        {
                            OnCertificateValidated(context, subjectPatterns);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            if (context.HttpContext.Connection.ClientCertificate == null)
                            {
                                Log.Warning("No client certificate");
                            }

                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCertificateCache(options =>
                {
                    options.CacheSize = authConfig.CertificateCacheSize;
                    options.CacheEntryExpiration = TimeSpan.FromSeconds(authConfig.CertificateCacheTtlSeconds);
                });

            services.AddAuthorization();
        }

        private static void OnCertificateValidated(CertificateValidatedContext context, HashSet<Regex>? subjectPatterns)
        {
            Claim[] claims =
            {
                new(ClaimTypes.NameIdentifier, context.ClientCertificate.Subject, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                new(ClaimTypes.Name, context.ClientCertificate.Subject, ClaimValueTypes.String, context.Options.ClaimsIssuer)
            };

            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));

            // If at least one subject patterns are defined, perform a regex match against the subject name for each pattern.
            // If a match is found, the request is authorised.
            bool authorised = true;
            if (subjectPatterns?.Count > 0)
            {
                authorised = subjectPatterns.Any(pattern => pattern.IsMatch(context.ClientCertificate.Subject));
            }

            if (!authorised)
            {
                Log.Warning("Failed to validate client certificate {Subject}", context.ClientCertificate.Subject);
                context.Fail("Unauthorised");
            }
            else
            {
                context.Success();
            }
        }

        /// <summary>
        /// Allow the HTTP context to be accessed for the purpose of logging user claims
        /// </summary>
        private static void ConfigureAuthorisation(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
