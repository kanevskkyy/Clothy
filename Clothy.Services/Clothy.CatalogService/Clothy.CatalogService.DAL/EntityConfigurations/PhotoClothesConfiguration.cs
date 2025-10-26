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
    public class PhotoClothesConfiguration : IEntityTypeConfiguration<PhotoClothes>
    {
        public void Configure(EntityTypeBuilder<PhotoClothes> builder)
        {
            builder.ToTable("photo_clothes");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.PhotoURL)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(property => property.Clothe)
                .WithMany(property => property.Photos)
                .HasForeignKey(property => property.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.ClotheId);
        }
    }
}
