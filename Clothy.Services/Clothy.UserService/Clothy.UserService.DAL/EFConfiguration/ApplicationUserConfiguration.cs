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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(proprty => proprty.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(property => property.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(property => property.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(property => property.PhotoUrl)
                .IsRequired();

            builder.Property(property => property.PhoneNumber)
                .IsRequired();

            builder.HasIndex(property => property.Email)
                .IsUnique();

            builder.HasMany(property => property.RefreshTokens)
                .WithOne(property => property.User)
                .HasForeignKey(property => property.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
