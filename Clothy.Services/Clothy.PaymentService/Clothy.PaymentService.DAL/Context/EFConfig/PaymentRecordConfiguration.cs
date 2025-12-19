using Clothy.PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.DAL.Context.Config
{
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecordEF>
    {
        public void Configure(EntityTypeBuilder<PaymentRecordEF> builder)
        {
            builder.ToTable("payment_records");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.OrderId)
                .IsRequired();

            builder.Property(property => property.UserId)
                .IsRequired();

            builder.Property(property => property.Price)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(property => property.Status)
                .IsRequired();
            
            builder.Property(property => property.TransactionId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(property => property.CreatedAt)
                .IsRequired();
        }
    }
}
