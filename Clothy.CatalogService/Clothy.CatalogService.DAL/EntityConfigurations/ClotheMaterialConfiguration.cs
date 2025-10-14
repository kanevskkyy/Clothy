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
    public class ClotheMaterialConfiguration : IEntityTypeConfiguration<ClotheMaterial>
    {
        public void Configure(EntityTypeBuilder<ClotheMaterial> builder)
        {
            builder.ToTable("clothe_materials");

            builder.HasKey(property => new 
            {
                property.ClotheId, 
                property.MaterialId 
            });

            builder.Property(property => property.Percentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired()
                .HasColumnName("percentage");

            builder.HasOne(property => property.Clothe)
                .WithMany(property => property.ClotheMaterials)
                .HasForeignKey(property => property.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(property => property.Material)
                .WithMany(property => property.ClotheMaterials)
                .HasForeignKey(property => property.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasCheckConstraint("ck_clothe_materials_percetage_valid", "\"percentage\" >= 0 AND \"percentage\" <= 100");
        }
    }
}
