namespace GymManager.Api.Domain.Entities;

public class ClassType
{
    public Guid ClassTypeId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}