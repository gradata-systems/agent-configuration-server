using ACS.Admin.Configuration;
using ACS.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.ResponseCompression;
using NuGet.Packaging;
using Serilog;
using System.IO.Compression;

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

            // Enable gzip response compression
            app.UseResponseCompression();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "admin",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );
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
