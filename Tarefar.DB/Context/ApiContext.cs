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
