using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clothy.CatalogService.DAL.DB
{
    public class ClothyCatalogDbContextFactory : IDesignTimeDbContextFactory<ClothyCatalogDbContext>
    {
        public ClothyCatalogDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ClothyCatalogDbContext> optionsBuilder = new DbContextOptionsBuilder<ClothyCatalogDbContext>();

            const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=ClothyCatalog;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(CONNECTION_STRING);

            return new ClothyCatalogDbContext(optionsBuilder.Options);
        }
    }
}
