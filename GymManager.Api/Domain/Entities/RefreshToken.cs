namespace GymManager.Api.Domain.Entities
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }

        public byte[] TokenHash { get; set; } = default!; // hash, nie plaintext
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }

        public User User { get; set; } = default!;
    }
}
