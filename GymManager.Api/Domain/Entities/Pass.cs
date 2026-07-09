namespace GymManager.Api.Domain.Entities;

public class Pass
{
    public Guid PassId { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid PassTypeId { get; set; }
    public PassType PassType { get; set; } = default!;

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public string Status { get; set; } = "active";

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}