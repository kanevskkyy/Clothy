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
    public class ClothingTypeConfiguration : IEntityTypeConfiguration<ClothingType>
    {
        public void Configure(EntityTypeBuilder<ClothingType> builder)
        {
            builder.ToTable("clothing_types"); 
            
            builder.HasKey(x => x.Id); 
            
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(100); 
            
            builder.HasIndex(p => p.Slug)
                .IsUnique();

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasMany(x => x.Items)
                .WithOne(ci => ci.ClothyType)
                .HasForeignKey(ci => ci.ClothingTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
