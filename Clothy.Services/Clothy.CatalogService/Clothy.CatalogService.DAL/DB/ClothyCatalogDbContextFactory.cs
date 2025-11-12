using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Clothy.CatalogService.DAL.DB
{
    public class ClothyCatalogDbContextFactory : IDesignTimeDbContextFactory<ClothyCatalogDbContext>
    {
        public ClothyCatalogDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables() 
                .Build();

            string? connectionString = configuration["ClothyCatalogDb"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ClothyCatalogDb' is not set in environment variables!");
            }

            Console.WriteLine($"Using connection string: {connectionString}");

            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder<ClothyCatalogDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ClothyCatalogDbContext(optionsBuilder.Options);
        }
    }
}
