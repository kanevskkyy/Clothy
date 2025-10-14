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
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.ToTable("collections");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.Slug)
                .HasMaxLength(60);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(property => property.Slug)
                .IsUnique();

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasMany(property => property.ClotheItems)
                .WithOne(clothe => clothe.Collection)
                .HasForeignKey(clothe => clothe.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
