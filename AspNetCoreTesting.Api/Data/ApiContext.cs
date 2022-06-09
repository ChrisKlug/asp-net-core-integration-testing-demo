using AspNetCoreTesting.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTesting.Api.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(x => {
                x.ToTable("Users");
            });
        }

        #nullable disable
        public DbSet<User> Users { get; set; }
        #nullable enable
    }
}
