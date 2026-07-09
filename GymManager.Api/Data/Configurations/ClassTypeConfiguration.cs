using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class ClassTypeConfiguration : IEntityTypeConfiguration<ClassType>
{
    public void Configure(EntityTypeBuilder<ClassType> builder)
    {
        builder.ToTable("ClassTypes");
        builder.HasKey(x => x.ClassTypeId);

        builder.Property(x => x.ClassTypeId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);
        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.Name)
            .IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}