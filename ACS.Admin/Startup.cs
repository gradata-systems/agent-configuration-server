using ACS.Admin.Configuration;
using ACS.Shared;
using ACS.Shared.Configuration;
using ACS.Shared.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;
using NuGet.Packaging;
using Serilog;
using System.Security.Cryptography.X509Certificates;

namespace ACS.Admin
{
    public class Startup
    {
        public static readonly string ConfigurationRoot = "Admin";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDbContext>();

            // API services, required for simulation lookup
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITargetMatchingService, TargetMatchingService>();

            ConfigureAuthentication(services);
            ConfigureAuthorisation(services);
            
            // Add services to the container.
            services
                .AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            services.AddRazorPages();

            // Bind app settings to make them available via dependency injection
            services.AddOptions();
            services.Configure<DataSourceConfiguration>(Configuration.GetSection("DataSource"));
            services.Configure<CacheConfiguration>(Configuration.GetSection("Cache"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "admin",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

                routes.MapRazorPages();
            });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            OpenIdConfiguration? config = Configuration.GetRequiredSection(ConfigurationRoot)
                .GetRequiredSection("Authentication")
                .GetRequiredSection("OpenId").Get<OpenIdConfiguration>();
            if (config != null)
            {
                services
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

                        // If a custom CA cert is specified, perform custom certificate validation
                        if (!string.IsNullOrEmpty(config.CaTrustPath))
                        {
                            options.BackchannelHttpHandler = new HttpClientHandler()
                            {
                                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                                {
                                    using X509Certificate2 caCert = X509CertificateLoader.LoadCertificateFromFile(config.CaTrustPath);
                                    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                                    chain.ChainPolicy.CustomTrustStore.Add(caCert);

                                    return chain.Build(cert);
                                }
                            };
                        }
                    });
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
