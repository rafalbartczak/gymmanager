using GymManager.Api.Data;
using GymManager.Api.Dtos.Profile;
using GymManager.Api.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwords;

    public ProfileController(AppDbContext db, PasswordService passwords)
    {
        _db = db;
        _passwords = passwords;
    }

    // GET /profile - pobierz dane profilu
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = GetUserIdOrThrow();

        var user = await _db.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => new ProfileDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                TermsAcceptedAt = u.TermsAcceptedAt,
                PrivacyAcceptedAt = u.PrivacyAcceptedAt,
                MarketingConsent = u.MarketingConsent,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user is null) return NotFound();

        return Ok(user);
    }

    // PUT /profile - edycja profilu (imię, nazwisko, hasło, marketing)
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest req)
    {
        var userId = GetUserIdOrThrow();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user is null) return NotFound();

        // Imię
        if (req.FirstName is not null)
        {
            if (string.IsNullOrWhiteSpace(req.FirstName) || req.FirstName.Trim().Length > 100)
                return BadRequest("Imię jest wymagane (maks. 100 znaków).");
            user.FirstName = req.FirstName.Trim();
        }

        // Nazwisko
        if (req.LastName is not null)
        {
            if (string.IsNullOrWhiteSpace(req.LastName) || req.LastName.Trim().Length > 100)
                return BadRequest("Nazwisko jest wymagane (maks. 100 znaków).");
            user.LastName = req.LastName.Trim();
        }

        // Zmiana hasła
        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(req.CurrentPassword))
                return BadRequest("Podaj aktualne hasło, aby je zmienić.");

            if (!_passwords.Verify(user, req.CurrentPassword))
                return BadRequest("Aktualne hasło jest nieprawidłowe.");

            if (req.NewPassword.Length < 6)
                return BadRequest("Nowe hasło musi mieć minimum 6 znaków.");

            user.PasswordHash = _passwords.HashPassword(user, req.NewPassword);
        }

        // Zgoda marketingowa
        user.MarketingConsent = req.MarketingConsent;
        user.MarketingConsentAt = req.MarketingConsent ? DateTime.UtcNow : null;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // GET /profile/export - eksport danych (RODO - prawo do przenoszenia)
    [HttpGet("export")]
    public async Task<ActionResult<UserDataExport>> ExportData()
    {
        var userId = GetUserIdOrThrow();

        var user = await _db.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user is null) return NotFound();

        // Pobierz wszystkie dane użytkownika
        var passes = await _db.Passes
            .Where(p => p.UserId == userId)
            .Select(p => new ExportPassItem
            {
                PassId = p.PassId,
                PassTypeName = p.PassType.Name,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var payments = await _db.Payments
            .Where(p => p.UserId == userId)
            .Select(p => new ExportPaymentItem
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                ProviderName = p.ProviderName,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();

        var reservations = await _db.ClassReservations
            .Where(r => r.UserId == userId)
            .Select(r => new ExportReservationItem
            {
                ClassSessionId = r.ClassSessionId,
                ClassTypeName = r.ClassSession.ClassType.Name,
                SessionStartAt = r.ClassSession.StartAt,
                SessionEndAt = r.ClassSession.EndAt,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                CanceledAt = r.CanceledAt
            })
            .ToListAsync();

        var entries = await _db.Entries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EntryAt)
            .Select(e => new ExportEntryItem
            {
                EntryId = e.EntryId,
                EntryMethod = e.EntryMethod,
                PassTypeName = e.Pass != null ? e.Pass.PassType.Name : null,
                EntryAt = e.EntryAt,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        var export = new UserDataExport
        {
            ExportedAt = DateTime.UtcNow,
            User = new ExportUserData
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                TermsAccepted = user.TermsAccepted,
                TermsAcceptedAt = user.TermsAcceptedAt,
                PrivacyAccepted = user.PrivacyAccepted,
                PrivacyAcceptedAt = user.PrivacyAcceptedAt,
                MarketingConsent = user.MarketingConsent,
                MarketingConsentAt = user.MarketingConsentAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            },
            Passes = passes,
            Payments = payments,
            ClassReservations = reservations,
            Entries = entries
        };

        return Ok(export);
    }

    // DELETE /profile - usunięcie konta (RODO - prawo do bycia zapomnianym)
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserIdOrThrow();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user is null) return NotFound();

        // Soft delete - anonimizacja danych
        user.Email = $"deleted_{user.UserId}@anonymous.local";
        user.FirstName = "Usunięty";
        user.LastName = "Użytkownik";
        user.PasswordHash = "DELETED";
        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Unieważnij wszystkie refresh tokeny
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Konto zostało usunięte." });
    }

    private Guid GetUserIdOrThrow()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Brak userId w tokenie.");

        return userId;
    }
}