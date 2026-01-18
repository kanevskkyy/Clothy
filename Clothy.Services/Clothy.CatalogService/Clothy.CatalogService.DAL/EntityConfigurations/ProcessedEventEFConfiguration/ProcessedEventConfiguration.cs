using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clothy.CatalogService.DAL.EntityConfigurations.ProcessedEventEFConfiguration
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.ToTable("processed_events");

            builder.HasKey(property => property.EventId);

            builder.Property(property => property.ProcessedAt)
                .IsRequired();
        }
    }
}
