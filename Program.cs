using AgentConfigurationServer.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using NuGet.Packaging;

namespace AgentConfigurationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>();

            ConfigureAuthentication(builder);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

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
    }
}
