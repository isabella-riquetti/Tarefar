using Microsoft.Extensions.Configuration;
using Tarefar.DB.Models;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Tarefar.DB
{
    public class ApiContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public ApiContext(DbContextOptions<ApiContext> options)
          : base(options)
        { }

        public DbSet<Task> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>()
                .HasOne(x => x.User)
                .WithMany(x => x.Tasks)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Task>()
                .HasOne(x => x.ParentTask)
                .WithMany(x => x.Reocurrencies)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("ApiContext");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
