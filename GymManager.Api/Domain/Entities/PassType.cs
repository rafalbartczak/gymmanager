namespace GymManager.Api.Domain.Entities;

public class PassType
{
    public Guid PassTypeId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "PLN";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}