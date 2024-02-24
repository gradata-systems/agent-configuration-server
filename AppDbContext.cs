using AgentConfigurationServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentConfigurationServer
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
            string? connectionString = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseMySQL(connectionString);
            }
            else
            {
                throw new Exception("Database connection string not specified");
            }
        }
    }
}
