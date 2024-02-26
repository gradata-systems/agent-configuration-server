using ACS.Shared.Configuration;
using ACS.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ACS.Shared
{
    public class AppDbContext : DbContext
    {
        public DbSet<Target> Targets { get; set; }
        public DbSet<Fragment> Fragments { get; set; }
        public DbSet<TargetFragment> TargetFragments { get; set; }

        private readonly DataSourceConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IOptions<DataSourceConfiguration> config)
            : base(options)
        {
            _configuration = config.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_configuration.GetConnectionString());
        }
    }
}
