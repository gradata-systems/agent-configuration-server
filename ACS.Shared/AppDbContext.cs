using ACS.Shared.Configuration;
using ACS.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ACS.Shared
{
    public class AppDbContext : DbContext
    {
        public DbSet<Target> Targets { get; set; }
        public DbSet<Fragment> Fragments { get; set; }
        public DbSet<TargetFragment> TargetFragments { get; set; }

        private readonly IConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config)
            : base(options)
        {
            _configuration = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            DataSourceConfiguration? config = _configuration.GetRequiredSection("DataSource").Get<DataSourceConfiguration>();
            if (config != null)
            {
                optionsBuilder.UseMySQL(config.GetConnectionString());
            }
            else
            {
                throw new Exception("Data source configuration not specified");
            }
        }
    }
}
