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
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.ToTable("materials");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(property => property.Slug)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.UpdatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(property => property.Slug)
                .IsUnique();

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasMany(property => property.ClotheMaterials)
                .WithOne(material => material.Material)
                .HasForeignKey(material => material.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
