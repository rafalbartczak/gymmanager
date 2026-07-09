using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.PaymentId);

        builder.Property(x => x.PaymentId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.ProviderName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.ProviderOrderId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(x => x.Amount)
            .HasColumnType("decimal(10,2)");
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ProviderOrderId)
            .IsUnique();
        builder.HasIndex(x => x.Status);
    }
}