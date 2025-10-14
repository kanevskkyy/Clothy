using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;

namespace Clothy.CatalogService.SeedData.SeedData
{
    public interface ISeeder
    {
        Task SeedAsync(ClothyCatalogDbContext context);
    }
}
