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
    public class ColorConfiguration : IEntityTypeConfiguration<Color>
    {
        public void Configure(EntityTypeBuilder<Color> builder)
        {
            builder.ToTable("colors");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.HexCode)
                .IsRequired()
                .HasColumnName("hexcode")
                .HasMaxLength(7);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasMany(property => property.ClothesStocks)
                .WithOne(stock => stock.Color)
                .HasForeignKey(stock => stock.ColorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.HexCode)
                .IsUnique();

            builder.HasCheckConstraint("CK_Color_HexCode", "\"hexcode\" ~ '^#[0-9A-Fa-f]{6}$'");
        }
    }
}
