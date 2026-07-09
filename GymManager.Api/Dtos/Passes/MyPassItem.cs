namespace GymManager.Api.Dtos.Passes;

public class MyPassItem
{
    public Guid PassId { get; set; }
    public string PassTypeName { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = default!;
}