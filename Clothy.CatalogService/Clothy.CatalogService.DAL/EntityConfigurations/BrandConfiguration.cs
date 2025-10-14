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

            builder.Property(property => property.Slug)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.PhotoURL)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasIndex(property => property.Slug)
                .IsUnique();

            builder.HasMany(property => property.ClotheItems)
                .WithOne(property => property.Brand)
                .HasForeignKey(property => property.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
