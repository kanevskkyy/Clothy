using Clothy.CatalogService.Domain.Entities.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.EntityConfigurations.StockEFConfigiration
{
    public class StockNotificationConfiguration : IEntityTypeConfiguration<StockNotification>
    {
        public void Configure(EntityTypeBuilder<StockNotification> builder)
        {
            builder.ToTable("stock_notifications");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.Property(property => property.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(property => property.UserId)
                .IsRequired();

            builder.Property(property => property.UserEmail)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.UserFirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.IsNotified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(property => property.Stock)
                .WithMany(property => property.StockNotifications)
                .HasForeignKey(property => property.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(property => property.UserId);

            builder.HasIndex(property => new
            {
                property.StockId,
                property.UserEmail
            }).IsUnique();
        }
    }
}
