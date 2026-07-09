using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Entries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("entries")]
public class EntriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EntriesController(AppDbContext db) => _db = db;

    // =====================
    // USER: historia swoich wejść
    // =====================

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<EntryDto>>> GetMyEntries(
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = GetUserIdOrThrow();

        var query = _db.Entries
            .Where(e => e.UserId == userId);

        if (from.HasValue)
            query = query.Where(e => e.EntryAt >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.EntryAt <= to.Value);

        var entries = await query
            .OrderByDescending(e => e.EntryAt)
            .Take(limit)
            .Select(e => new EntryDto
            {
                EntryId = e.EntryId,
                UserId = e.UserId,
                UserEmail = e.User.Email,
                EntryMethod = e.EntryMethod,
                PassId = e.PassId,
                PassTypeName = e.Pass != null ? e.Pass.PassType.Name : null,
                EntryAt = e.EntryAt
            })
            .ToListAsync();

        return Ok(entries);
    }

    // USER: samoobsługowe wejście (skanowanie kodu klubu)
    [Authorize]
    [HttpPost("checkin")]
    public async Task<ActionResult<EntryDto>> CheckIn(CheckInRequest req)
    {
        var userId = GetUserIdOrThrow();

        // Weryfikuj kod klubu (prosty statyczny kod - w produkcji byłby bardziej złożony)
        if (string.IsNullOrWhiteSpace(req.ClubCode) || req.ClubCode != "GYMMANAGER2026")
        {
            return BadRequest("Nieprawidłowy kod klubu.");
        }

        // Sprawdź czy user ma aktywny karnet
        var now = DateTime.UtcNow;
        var activePass = await _db.Passes
            .Where(p => p.UserId == userId && p.Status == "active" && p.EndAt > now)
            .OrderByDescending(p => p.EndAt)
            .FirstOrDefaultAsync();

        if (activePass is null)
        {
            return Conflict(new { error = "NO_ACTIVE_PASS", message = "Nie posiadasz aktywnego karnetu." });
        }

        // Sprawdź czy nie było wejścia w ciągu ostatnich 5 minut (zabezpieczenie przed duplikatami)
        var recentEntry = await _db.Entries
            .Where(e => e.UserId == userId && e.EntryAt > now.AddMinutes(-5))
            .AnyAsync();

        if (recentEntry)
        {
            return Conflict(new { error = "RECENT_ENTRY", message = "Wejście zostało już zarejestrowane." });
        }

        var entry = new Entry
        {
            UserId = userId,
            EntryMethod = "qr_scan",
            PassId = activePass.PassId,
            EntryAt = now,
            CreatedAt = now
        };

        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(new EntryDto
        {
            EntryId = entry.EntryId,
            UserId = entry.UserId,
            UserEmail = (await _db.Users.FindAsync(userId))!.Email,
            EntryMethod = entry.EntryMethod,
            PassId = entry.PassId,
            PassTypeName = activePass.PassType?.Name,
            EntryAt = entry.EntryAt
        });
    }

    // =====================
    // ADMIN: historia wszystkich wejść
    // =====================

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ActionResult<List<EntryDto>>> GetAllEntries(
        [FromQuery] int limit = 100,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? userId = null)
    {
        var query = _db.Entries.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.EntryAt >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.EntryAt <= to.Value);
        if (userId.HasValue)
            query = query.Where(e => e.UserId == userId.Value);

        var entries = await query
            .OrderByDescending(e => e.EntryAt)
            .Take(limit)
            .Select(e => new EntryDto
            {
                EntryId = e.EntryId,
                UserId = e.UserId,
                UserEmail = e.User.Email,
                EntryMethod = e.EntryMethod,
                PassId = e.PassId,
                PassTypeName = e.Pass != null ? e.Pass.PassType.Name : null,
                EntryAt = e.EntryAt,
                RegisteredByAdminId = e.RegisteredByAdminId
            })
            .ToListAsync();

        return Ok(entries);
    }

    // ADMIN: weryfikacja kodu QR użytkownika i rejestracja wejścia
    [Authorize(Roles = "admin")]
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyEntryResponse>> VerifyAndRegister(VerifyEntryRequest req)
    {
        var adminId = GetUserIdOrThrow();

        User? user = null;

        // Sprawdź czy to GUID (kod QR) czy email
        if (Guid.TryParse(req.UserCode, out var targetUserId))
        {
            // Szukaj po GUID
            user = await _db.Users
                .Where(u => u.UserId == targetUserId && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }
        else if (req.UserCode.Contains("@"))
        {
            // Szukaj po email
            user = await _db.Users
                .Where(u => u.Email.ToLower() == req.UserCode.ToLower() && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }
        else
        {
            return BadRequest(new VerifyEntryResponse
            {
                Success = false,
                Error = "INVALID_CODE",
                Message = "Podaj prawidłowy kod QR lub adres email."
            });
        }

        if (user is null)
        {
            return NotFound(new VerifyEntryResponse
            {
                Success = false,
                Error = "USER_NOT_FOUND",
                Message = "Nie znaleziono użytkownika."
            });
        }

        var now = DateTime.UtcNow;
        var activePass = await _db.Passes
            .Include(p => p.PassType)
            .Where(p => p.UserId == user.UserId && p.Status == "active" && p.EndAt > now)
            .OrderByDescending(p => p.EndAt)
            .FirstOrDefaultAsync();

        if (activePass is null)
        {
            return Ok(new VerifyEntryResponse
            {
                Success = false,
                Error = "NO_ACTIVE_PASS",
                Message = "Użytkownik nie posiada aktywnego karnetu.",
                UserEmail = user.Email,
                UserId = user.UserId
            });
        }

        // Sprawdź czy nie było wejścia w ciągu ostatnich 5 minut (zabezpieczenie przed duplikatami)
        var recentEntry = await _db.Entries
            .Where(e => e.UserId == user.UserId && e.EntryAt > now.AddMinutes(-5))
            .FirstOrDefaultAsync();

        if (recentEntry is not null)
        {
            return Ok(new VerifyEntryResponse
            {
                Success = false,
                Error = "RECENT_ENTRY",
                Message = $"Wejście już zarejestrowane o {recentEntry.EntryAt.ToLocalTime():HH:mm}.",
                UserEmail = user.Email,
                UserId = user.UserId
            });
        }

        // Rejestruj wejście
        var entry = new Entry
        {
            UserId = user.UserId,
            EntryMethod = "admin_scan",
            PassId = activePass.PassId,
            RegisteredByAdminId = adminId,
            EntryAt = now,
            CreatedAt = now
        };

        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(new VerifyEntryResponse
        {
            Success = true,
            Message = "Wejście zarejestrowane.",
            UserEmail = user.Email,
            UserId = user.UserId,
            PassTypeName = activePass.PassType?.Name,
            PassValidUntil = activePass.EndAt,
            EntryId = entry.EntryId
        });
    }

    // ADMIN: ręczna rejestracja wejścia
    [Authorize(Roles = "admin")]
    [HttpPost("manual")]
    public async Task<ActionResult<EntryDto>> ManualEntry(ManualEntryRequest req)
    {
        var adminId = GetUserIdOrThrow();

        var user = await _db.Users
            .Where(u => u.UserId == req.UserId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound("Nie znaleziono użytkownika.");

        var now = DateTime.UtcNow;

        // Sprawdź czy nie było wejścia w ciągu ostatnich 5 minut (zabezpieczenie przed duplikatami)
        var recentEntry = await _db.Entries
            .Where(e => e.UserId == req.UserId && e.EntryAt > now.AddMinutes(-5))
            .AnyAsync();

        if (recentEntry)
        {
            return Conflict(new { error = "RECENT_ENTRY", message = "Wejście już zostało zarejestrowane w ciągu ostatnich 5 minut." });
        }

        // Opcjonalnie znajdź aktywny karnet
        var activePass = await _db.Passes
            .Where(p => p.UserId == req.UserId && p.Status == "active" && p.EndAt > now)
            .FirstOrDefaultAsync();

        var entry = new Entry
        {
            UserId = req.UserId,
            EntryMethod = "manual",
            PassId = activePass?.PassId,
            RegisteredByAdminId = adminId,
            EntryAt = now,
            CreatedAt = now
        };

        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(new EntryDto
        {
            EntryId = entry.EntryId,
            UserId = entry.UserId,
            UserEmail = user.Email,
            EntryMethod = entry.EntryMethod,
            PassId = entry.PassId,
            EntryAt = entry.EntryAt,
            RegisteredByAdminId = adminId
        });
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