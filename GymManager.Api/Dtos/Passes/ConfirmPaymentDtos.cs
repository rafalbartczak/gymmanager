namespace GymManager.Api.Dtos.Passes;

public class ConfirmPaymentRequest
{
    public Guid PaymentId { get; set; }
    public Guid PassTypeId { get; set; }
}

public class ConfirmPaymentResponse
{
    public Guid PassId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string PaymentStatus { get; set; } = default!;
}