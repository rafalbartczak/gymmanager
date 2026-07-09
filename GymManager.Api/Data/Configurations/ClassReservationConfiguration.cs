using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class ClassReservationConfiguration : IEntityTypeConfiguration<ClassReservation>
{
    public void Configure(EntityTypeBuilder<ClassReservation> builder)
    {
        builder.ToTable("ClassReservations");
        builder.HasKey(x => x.ClassReservationId);

        builder.Property(x => x.ClassReservationId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ClassSession)
            .WithMany()
            .HasForeignKey(x => x.ClassSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        // 1 user może mieć max 1 rezerwację na sesję (a cancel robimy statusem)
        builder.HasIndex(x => new { x.UserId, x.ClassSessionId }).IsUnique();

        builder.HasIndex(x => x.ClassSessionId);
        builder.HasIndex(x => x.Status);
    }
}