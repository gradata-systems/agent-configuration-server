using ACS.Api.Configuration;
using ACS.Api.Services;
using ACS.Shared;
using ACS.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;

namespace ACS.Admin
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
            services.AddResponseCompression();

            // Add services to the container.
            services.AddControllersWithViews();

            // Bind app settings to make them available via dependency injection
            services.AddOptions();
            services.Configure<DataSourceConfiguration>(Configuration.GetSection("DataSource"));
            services.Configure<ApiConfiguration>(Configuration.GetSection(ConfigurationRoot));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            string? caTrustPath = Configuration.GetRequiredSection(ConfigurationRoot)
                .GetRequiredSection("Authentication")
                .GetValue<string>("CaTrustPath");

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.RevocationMode = X509RevocationMode.NoCheck;

                    if (!string.IsNullOrEmpty(caTrustPath))
                    {
                        options.CustomTrustStore = new X509Certificate2Collection(X509Certificate2.CreateFromPemFile(caTrustPath));
                    }
                });
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
