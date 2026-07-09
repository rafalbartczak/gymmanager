namespace GymManager.Client.Contracts;

public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    bool TermsAccepted,
    bool PrivacyAccepted,
    bool MarketingConsent
);