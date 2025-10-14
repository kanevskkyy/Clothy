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
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("brands");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.PhotoURL)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasMany(property => property.ClotheItems)
                .WithOne(clothe => clothe.Brand)
                .HasForeignKey(clothe => clothe.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
