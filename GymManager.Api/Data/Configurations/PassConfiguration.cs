using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class PassConfiguration : IEntityTypeConfiguration<Pass>
{
    public void Configure(EntityTypeBuilder<Pass> builder)
    {
        builder.ToTable("Passes");
        builder.HasKey(x => x.PassId);

        builder.Property(x => x.PassId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.Status).IsRequired().HasMaxLength(20);

        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.PassType)
            .WithMany()
            .HasForeignKey(x => x.PassTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Payment)
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PassTypeId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EndAt);
    }
}