using ACS.Shared;
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
            services.AddDbContext<AppDbContext>();

            ConfigureAuthentication(services);
            ConfigureAuthorisation(services);

            // Response compression
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression();

            // Add services to the container.
            services.AddControllersWithViews();
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
