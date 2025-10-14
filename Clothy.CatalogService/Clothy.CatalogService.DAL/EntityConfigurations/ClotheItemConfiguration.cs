using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clothy.CatalogService.DAL.EntityConfigurations
{
    public class ClotheItemConfiguration : IEntityTypeConfiguration<ClotheItem>
    {
        public void Configure(EntityTypeBuilder<ClotheItem> builder)
        {
            builder.ToTable("clothe_items");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.Slug)
                .IsRequired()
                .HasMaxLength(60);

            builder.HasIndex(property => property.Slug)
                .IsUnique();

            builder.Property(property => property.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(property => property.MainPhotoURL)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(property => property.Price)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.UpdatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(property => property.Collection)
                .WithMany(col => col.ClotheItems)
                .HasForeignKey(property => property.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(property => property.Photos)
                .WithOne(photo => photo.Clothe)
                .HasForeignKey(photo => photo.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(property => property.Stocks)
                .WithOne(stock => stock.Clothe)
                .HasForeignKey(stock => stock.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(property => property.ClotheTags)
                .WithOne(tag => tag.Clothe)
                .HasForeignKey(tag => tag.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(property => property.ClotheMaterials)
                .WithOne(material => material.Clothe)
                .HasForeignKey(material => material.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(property => property.Brand)
                .WithMany(brand => brand.ClotheItems)
                .HasForeignKey(property => property.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.Name);
            builder.HasIndex(property => property.Price);
        }
    }
}
