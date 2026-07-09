namespace GymManager.Client.Contracts;

public record PassTypeListItem(
    Guid PassTypeId,
    string Name,
    string? Description,
    int DurationDays,
    decimal Price,
    string Currency
);

public record BuyPassRequest(Guid PassTypeId);

public record BuyPassResponse(
    Guid PaymentId,
    Guid PassTypeId,
    string PassTypeName,
    decimal Amount,
    string Currency,
    string PaymentStatus,
    string ProviderName,
    string ProviderOrderId
);

public record ConfirmPaymentRequest(Guid PaymentId, Guid PassTypeId);

public record ConfirmPaymentResponse(
    Guid PassId,
    DateTime StartAt,
    DateTime EndAt,
    string PaymentStatus
);

public record MyPassItem(
    Guid PassId,
    string PassTypeName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    decimal Price,
    string Currency
);