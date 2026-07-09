using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManager.Api.Data.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.ToTable("Entries");

        builder.HasKey(e => e.EntryId);
        builder.Property(e => e.EntryId).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.EntryMethod)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("manual");

        builder.Property(e => e.EntryAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Relacja z User (kto wszedł)
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relacja z Pass (opcjonalna)
        builder.HasOne(e => e.Pass)
            .WithMany()
            .HasForeignKey(e => e.PassId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relacja z Admin (opcjonalna)
        builder.HasOne(e => e.RegisteredByAdmin)
            .WithMany()
            .HasForeignKey(e => e.RegisteredByAdminId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indeksy
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EntryAt);
        builder.HasIndex(e => e.PassId);
    }
}