using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.EntityConfigurations
{
    public class ClothesStockConfiguration : IEntityTypeConfiguration<ClothesStock>
    {
        public void Configure(EntityTypeBuilder<ClothesStock> builder)
        {
            builder.ToTable("clothes_stock");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Quantity)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("quantity");

            builder.HasCheckConstraint("ck_clothes_stock_quantity_valid", "\"quantity\" >= 0");

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(property => property.Clothe)
                .WithMany(property => property.Stocks)
                .HasForeignKey(property => property.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(property => property.Size)
                .WithMany(property => property.ClothesStocks)
                .HasForeignKey(property => property.SizeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(property => property.Color)
                .WithMany(property => property.ClothesStocks)
                .HasForeignKey(property => property.ColorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => new 
            { 
                property.ClotheId, 
                property.SizeId, 
                property.ColorId 
            }).IsUnique();

        }
    }
}
