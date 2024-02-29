using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ACS.Shared.Utilities
{
    public static class DbUtils
    {
        /// <summary>
        /// Apply any outstanding migrations
        /// </summary>
        public static void ApplyMigrations(IHost host)
        {
            using (IServiceScope scope = host.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
        }
    }
}
