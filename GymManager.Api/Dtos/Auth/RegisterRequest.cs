namespace GymManager.Api.Dtos.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool TermsAccepted { get; set; }
        public bool PrivacyAccepted { get; set; }
        public bool MarketingConsent { get; set; }
    }
}