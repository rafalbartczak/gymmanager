namespace GymManager.Api.Dtos.Passes;

public class BuyPassResponse
{
    public Guid PaymentId { get; set; }
    public Guid PassTypeId { get; set; }
    public string PassTypeName { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public string ProviderOrderId { get; set; } = default!;
}