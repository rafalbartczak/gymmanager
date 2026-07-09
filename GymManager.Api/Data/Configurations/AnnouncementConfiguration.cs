using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Data.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("Announcements");

            builder.HasKey(a => a.AnnouncementId);

            builder.Property(a => a.AnnouncementId)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Content)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(a => a.IsPublished)
                .HasDefaultValue(true);

            builder.Property(a => a.PublishedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(a => a.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(a => a.CreatedByAdminId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(a => a.PublishedAt);
            builder.HasIndex(a => a.IsPublished);
        }
    }
}
