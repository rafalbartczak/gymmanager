using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class PassTypeConfiguration : IEntityTypeConfiguration<PassType>
{
    public void Configure(EntityTypeBuilder<PassType> builder)
    {
        builder.ToTable("PassTypes");
        builder.HasKey(x => x.PassTypeId);

        builder.Property(x => x.PassTypeId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.Name)
            .IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DurationDays)
            .IsRequired();
        builder.Property(x => x.Price)
            .HasColumnType("decimal(10,2)");
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}