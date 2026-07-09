namespace GymManager.Api.Dtos.Auth
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
