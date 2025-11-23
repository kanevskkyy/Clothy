using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Clothy.UserService.DAL.EFConfiguration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(property => property.Id);

            builder.Property(property => property.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(property => property.CreatedAt)
                .IsRequired();

            builder.HasOne(property => property.User)
                .WithMany(property => property.RefreshTokens)
                .HasForeignKey(property => property.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
