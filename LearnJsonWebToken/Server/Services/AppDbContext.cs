using LearnJsonWebToken.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnJsonWebToken.Server.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(x => x.DisplayName).IsUnique();
        }

        public DbSet<User> Users => Set<User>();
    }
}
