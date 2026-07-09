namespace GymManager.Api.Dtos.Announcements
{
    public class CreateAnnouncementRequest
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsPublished { get; set; } = true;
    }
}
