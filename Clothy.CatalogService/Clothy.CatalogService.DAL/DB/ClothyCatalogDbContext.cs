using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.EntityConfigurations;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.DB
{
    public class ClothyCatalogDbContext : DbContext
    {
        public ClothyCatalogDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<ClotheItem> ClotheItems { get; set; }
        public DbSet<ClotheMaterial> ClotheMaterials { get; set; }
        public DbSet<ClothesStock> ClothesStocks { get; set; }
        public DbSet<ClotheTag> ClotheTags { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<PhotoClothes> PhotoClothes { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BrandConfiguration());
            modelBuilder.ApplyConfiguration(new ClotheItemConfiguration());
            modelBuilder.ApplyConfiguration(new ClotheMaterialConfiguration());
            modelBuilder.ApplyConfiguration(new ClothesStockConfiguration());
            modelBuilder.ApplyConfiguration(new ClotheTagConfiguration());
            modelBuilder.ApplyConfiguration(new CollectionConfiguration());
            modelBuilder.ApplyConfiguration(new ColorConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoClothesConfiguration());
            modelBuilder.ApplyConfiguration(new SizeConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
