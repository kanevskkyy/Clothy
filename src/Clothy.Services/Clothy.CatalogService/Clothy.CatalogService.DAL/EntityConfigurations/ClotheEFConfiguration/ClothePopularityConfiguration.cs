using Clothy.CatalogService.Domain.Entities.Clothe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.EntityConfigurations.ClotheEFConfiguration
{
    public class ClothePopularityConfiguration : IEntityTypeConfiguration<ClothePopularity>
    {
        public void Configure(EntityTypeBuilder<ClothePopularity> builder)
        {
            builder.ToTable("clothe_popularities");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.ClotheId)
                .IsRequired();

            builder.Property(property => property.SoldCount)
                .IsRequired();

            builder.HasOne(property => property.ClotheItem)
                .WithMany(property => property.ClothePopularities)
                .HasForeignKey(property => property.ClotheId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.ClotheId)
                .IsUnique();
        }
    }
}
