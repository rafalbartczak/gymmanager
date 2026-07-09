namespace GymManager.Api.Domain.Entities
{
    public class Announcement
    {
        public Guid AnnouncementId { get; set; }

        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;

        public bool IsPublished { get; set; } = true;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedByAdminId { get; set; }
        public User CreatedByAdmin { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
