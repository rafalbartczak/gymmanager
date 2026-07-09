namespace GymManager.Client.Contracts
{
    public record AnnouncementListItem(
        Guid AnnouncementId,
        string Title,
        string Content,
        bool IsPublished,
        DateTime PublishedAt
    );
}
