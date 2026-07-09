namespace GymManager.Api.Domain.Entities;

public class ClassReservation
{
    public Guid ClassReservationId { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = default!;

    public string Status { get; set; } = "reserved";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set; }
}