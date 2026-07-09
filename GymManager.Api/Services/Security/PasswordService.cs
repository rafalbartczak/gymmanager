using Microsoft.AspNetCore.Identity;
using GymManager.Api.Domain.Entities;

namespace GymManager.Api.Services.Security
{
    /// <summary>
    /// Hashowanie i weryfikacja haseł algorytmem PBKDF2 z automatyczną solą.
    /// Wykorzystuje wbudowany PasswordHasher z ASP.NET Core Identity.
    /// </summary>
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string password)
            => _hasher.HashPassword(user, password);

        public bool Verify(User user, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success
                   || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}