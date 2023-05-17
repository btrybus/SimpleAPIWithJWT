using Microsoft.EntityFrameworkCore;

namespace SimpleAPIWithJWT.Models
{
    public class SecurityDbContext : DbContext
    {
        public SecurityDbContext
            (DbContextOptions<SecurityDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(p => p.Login)
                .IsUnique(true);
        }
    }
}
