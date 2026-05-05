using Clothy.CatalogService.DAL.DB;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.UnitTests.Helpers.Persistance;

public static class DbContextFactory
{
    public static ClothyCatalogDbContext CreateInMemoryContext(string? dbName = null)
    {
        DbContextOptions<ClothyCatalogDbContext> options = new DbContextOptionsBuilder<ClothyCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;
 
        return new ClothyCatalogDbContext(options);
    }
}