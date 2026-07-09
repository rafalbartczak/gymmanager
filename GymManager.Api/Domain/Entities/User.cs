namespace GymManager.Api.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public string Role { get; set; } = "user";
    public bool IsActive { get; set; } = true;

    public bool TermsAccepted { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public bool PrivacyAccepted { get; set; }
    public DateTime? PrivacyAcceptedAt { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime? MarketingConsentAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}