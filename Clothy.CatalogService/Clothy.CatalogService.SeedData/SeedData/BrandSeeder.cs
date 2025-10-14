//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Bogus;
//using Clothy.CatalogService.DAL.DB;
//using Clothy.CatalogService.Domain.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace Clothy.CatalogService.SeedData.SeedData
//{
//    public class BrandSeeder : ISeeder
//    {
//        public async Task SeedAsync(ClothyCatalogDbContext context)
//        {
//            if (await context.Brands.AnyAsync()) return;

//            Faker<Brand> faker = new Faker<Brand>()
//                .RuleFor(p => p.Id, p => Guid.NewGuid())
//                .RuleFor(p => p.CreatedAt, fakeData => fakeData.Date.Past(2).ToUniversalTime())

//        }
//    }
//}
