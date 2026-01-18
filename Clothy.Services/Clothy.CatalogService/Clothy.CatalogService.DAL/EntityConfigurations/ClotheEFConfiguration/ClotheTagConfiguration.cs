using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Clothy.CatalogService.Domain.Entities.Clothe;

namespace Clothy.CatalogService.DAL.EntityConfigurations.ClotheEFConfiguration
{
    public class ClotheTagConfiguration : IEntityTypeConfiguration<ClotheTag>
    {
        public void Configure(EntityTypeBuilder<ClotheTag> builder)
        {
            builder.ToTable("clothe_tags");

            builder.HasKey(property => new 
            { 
                property.ClotheId, 
                property.TagId 
            });

            builder.HasOne(property => property.Clothe)
                .WithMany(clothe => clothe.ClotheTags)
                .HasForeignKey(property => property.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(property => property.Tag)
                .WithMany(tag => tag.ClotheTags)
                .HasForeignKey(property => property.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.ClotheId);
            builder.HasIndex(property => property.TagId);
        }
    }

}
