using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.UserId);

            // DB generuje GUID sekwencyjny (lepsze pod indeksy niż NEWID)
            builder.Property(u => u.UserId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(320);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.MarketingConsent)
                .HasDefaultValue(false);

            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(u => u.UpdatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }
}