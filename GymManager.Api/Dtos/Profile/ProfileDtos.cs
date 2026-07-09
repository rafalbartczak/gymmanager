namespace GymManager.Api.Dtos.Profile;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public DateTime? PrivacyAcceptedAt { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public bool MarketingConsent { get; set; }
}

// Export DTOs
public class UserDataExport
{
    public DateTime ExportedAt { get; set; }
    public ExportUserData User { get; set; } = default!;
    public List<ExportPassItem> Passes { get; set; } = new();
    public List<ExportPaymentItem> Payments { get; set; } = new();
    public List<ExportReservationItem> ClassReservations { get; set; } = new();
    public List<ExportEntryItem> Entries { get; set; } = new();
}

public class ExportUserData
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool TermsAccepted { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public bool PrivacyAccepted { get; set; }
    public DateTime? PrivacyAcceptedAt { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime? MarketingConsentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExportPassItem
{
    public Guid PassId { get; set; }
    public string PassTypeName { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class ExportPaymentItem
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ExportReservationItem
{
    public Guid ClassSessionId { get; set; }
    public string ClassTypeName { get; set; } = default!;
    public DateTime SessionStartAt { get; set; }
    public DateTime SessionEndAt { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
}

public class ExportEntryItem
{
    public Guid EntryId { get; set; }
    public string EntryMethod { get; set; } = default!;
    public string? PassTypeName { get; set; }
    public DateTime EntryAt { get; set; }
    public DateTime CreatedAt { get; set; }
}