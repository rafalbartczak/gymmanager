using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");
        builder.HasKey(x => x.ClassSessionId);

        builder.Property(x => x.ClassSessionId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.StartAt)
            .IsRequired();
        builder.Property(x => x.EndAt)
            .IsRequired();
        builder.Property(x => x.Capacity)
            .IsRequired();
        builder.Property(x => x.IsCanceled)
            .HasDefaultValue(false);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.ClassType)
            .WithMany()
            .HasForeignKey(x => x.ClassTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ClassTypeId);
        builder.HasIndex(x => x.StartAt);
        builder.HasIndex(x => x.IsCanceled);
    }
}