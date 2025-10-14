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
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("tag");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.UpdatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(property => property.Name)
                .IsUnique();

            builder.HasMany(property => property.ClotheTags)
                .WithOne(tag => tag.Tag)
                .HasForeignKey(tag => tag.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
