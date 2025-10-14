using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.DAL.EntityConfigurations
{
    public class SizeConfiguration : IEntityTypeConfiguration<Size>
    {
        public void Configure(EntityTypeBuilder<Size> builder)
        {
            builder.ToTable("sizes");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.UpdatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasMany(property => property.ClothesStocks)
                .WithOne(stock => stock.Size)
                .HasForeignKey(stock => stock.SizeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
