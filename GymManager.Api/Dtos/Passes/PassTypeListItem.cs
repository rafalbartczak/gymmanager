namespace GymManager.Api.Dtos.Passes;

public class PassTypeListItem
{
    public Guid PassTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = default!;
}