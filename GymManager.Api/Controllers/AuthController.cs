using System.IdentityModel.Tokens.Jwt;
using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Auth;
using GymManager.Api.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _passwords;
    private readonly JwtTokenService _jwt;
    private readonly RefreshTokenService _refresh;

    public AuthController(AppDbContext db, PasswordService passwords, JwtTokenService jwt, RefreshTokenService refresh)
    {
        _db = db;
        _passwords = passwords;
        _jwt = jwt;
        _refresh = refresh;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (!req.TermsAccepted || !req.PrivacyAccepted)
            return BadRequest("Wymagane jest zaakceptowanie regulaminu i polityki prywatności.");

        if (string.IsNullOrWhiteSpace(req.FirstName) || req.FirstName.Trim().Length > 100)
            return BadRequest("Imię jest wymagane (maks. 100 znaków).");

        if (string.IsNullOrWhiteSpace(req.LastName) || req.LastName.Trim().Length > 100)
            return BadRequest("Nazwisko jest wymagane (maks. 100 znaków).");

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest("Hasło musi mieć minimum 6 znaków.");

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) return Conflict("Użytkownik o podanym adresie e-mail już istnieje.");

        var user = new User
        {
            Email = email,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Role = "user",
            IsActive = true,

            TermsAccepted = req.TermsAccepted,
            TermsAcceptedAt = DateTime.UtcNow,
            PrivacyAccepted = req.PrivacyAccepted,
            PrivacyAcceptedAt = DateTime.UtcNow,

            MarketingConsent = req.MarketingConsent,
            MarketingConsentAt = req.MarketingConsent ? DateTime.UtcNow : null,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwords.HashPassword(user, req.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // tokeny
        var access = _jwt.CreateAccessToken(user);
        var refreshPlain = _refresh.GenerateRefreshToken();
        var refreshHash = _refresh.ComputeTokenHash(refreshPlain);

        var refreshDays = int.Parse(HttpContext.RequestServices
            .GetRequiredService<IConfiguration>().GetSection("Jwt")["RefreshTokenDays"]!);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse { AccessToken = access, RefreshToken = refreshPlain });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        if (user is null) return Unauthorized("Nieprawidłowy e-mail lub hasło.");
        if (!user.IsActive) return Unauthorized("Konto jest nieaktywne.");

        var ok = _passwords.Verify(user, req.Password);
        if (!ok) return Unauthorized("Nieprawidłowy e-mail lub hasło.");

        var access = _jwt.CreateAccessToken(user);
        var refreshPlain = _refresh.GenerateRefreshToken();
        var refreshHash = _refresh.ComputeTokenHash(refreshPlain);

        var refreshDays = int.Parse(HttpContext.RequestServices
            .GetRequiredService<IConfiguration>().GetSection("Jwt")["RefreshTokenDays"]!);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse { AccessToken = access, RefreshToken = refreshPlain });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest req)
    {
        var hash = _refresh.ComputeTokenHash(req.RefreshToken);

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (token is null) return Unauthorized("Nieprawidłowy refresh token.");
        if (token.RevokedAt is not null) return Unauthorized("Refresh token został unieważniony.");
        if (token.ExpiresAt <= DateTime.UtcNow) return Unauthorized("Refresh token wygasł.");

        var user = await _db.Users.FirstAsync(u => u.UserId == token.UserId);
        if (user.IsDeleted || !user.IsActive) return Unauthorized("Konto jest nieaktywne.");

        // rotacja: unieważnij stary
        token.RevokedAt = DateTime.UtcNow;

        // wygeneruj nowy
        var access = _jwt.CreateAccessToken(user);
        var refreshPlain = _refresh.GenerateRefreshToken();
        var refreshHash = _refresh.ComputeTokenHash(refreshPlain);

        var refreshDays = int.Parse(HttpContext.RequestServices
            .GetRequiredService<IConfiguration>().GetSection("Jwt")["RefreshTokenDays"]!);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse { AccessToken = access, RefreshToken = refreshPlain });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest req)
    {
        var hash = _refresh.ComputeTokenHash(req.RefreshToken);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (token is null) return Ok(); // nie zdradzamy czy istniał

        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _db.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => new { u.UserId, u.Email, u.Role, u.IsActive })
            .FirstOrDefaultAsync();

        if (user is null) return Unauthorized();
        return Ok(user);
    }

}