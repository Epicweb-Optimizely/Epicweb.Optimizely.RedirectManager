using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Epicweb.Optimizely.RedirectManager
{
    public class RedirectDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public RedirectDbContext(IConfiguration config)
        {
            _configuration = config;
        }

        public virtual DbSet<RedirectRule> RedirectRules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                _configuration.GetConnectionString("EPiServerDB"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<RedirectRule>(entity =>
            //{
            //    entity.ToTable("SEO_Redirect");
            //    entity.HasKey(x => x.Id);
            //});
        }
    }
}
