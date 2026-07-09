using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.RefreshTokenId);

            builder.Property(rt => rt.RefreshTokenId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(rt => rt.TokenHash)
                .IsRequired()
                .HasMaxLength(64); // np. SHA-256 = 32 bytes, SHA-512 = 64 bytes

            builder.Property(rt => rt.IpAddress)
                .HasMaxLength(45); // IPv6 max

            builder.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.ExpiresAt);
        }
    }
}
