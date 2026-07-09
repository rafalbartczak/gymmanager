namespace GymManager.Api.Domain.Entities;

public class ClassSession
{
    public Guid ClassSessionId { get; set; }

    public Guid ClassTypeId { get; set; }
    public ClassType ClassType { get; set; } = default!;

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public int Capacity { get; set; }

    public bool IsCanceled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}