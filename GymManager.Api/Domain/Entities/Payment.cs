namespace GymManager.Api.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; set; }

    public string ProviderName { get; set; } = "MockPay";
    public string ProviderOrderId { get; set; } = default!;

    public string Status { get; set; } = "pending";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";

    public DateTime? CompletedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}