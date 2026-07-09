namespace GymManager.Api.Dtos.Announcements
{
    public class AnnouncementListItem
    {
        public Guid AnnouncementId { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }

    }
}
