using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClassesController(AppDbContext db) => _db = db;

    // =====================
    // USER: types + schedule
    // =====================

    // Typy zajęć widoczne dla usera (tylko aktywne)
    [Authorize]
    [HttpGet("types")]
    public async Task<ActionResult<List<ClassTypeDto>>> GetTypes()
    {
        var items = await _db.ClassTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new ClassTypeDto
            {
                ClassTypeId = t.ClassTypeId,
                Name = t.Name,
                Description = t.Description,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }

    // Wszystkie typy zajęć (admin) — włącznie z nieaktywnymi
    [Authorize(Roles = "admin")]
    [HttpGet("types/all")]
    public async Task<ActionResult<List<ClassTypeDto>>> GetAllTypes()
    {
        var items = await _db.ClassTypes
            .OrderBy(t => t.Name)
            .Select(t => new ClassTypeDto
            {
                ClassTypeId = t.ClassTypeId,
                Name = t.Name,
                Description = t.Description,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }

    // Harmonogram: user widzi tylko przyszłe i nieodwołane, admin może zobaczyć wszystko
    [Authorize]
    [HttpGet("schedule")]
    public async Task<ActionResult<List<ClassSessionDto>>> GetSchedule(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool includeCanceled = false,
        [FromQuery] bool includePast = false)
    {
        var userId = GetUserIdOrThrow();

        var q = _db.ClassSessions.AsQueryable();

        if (!includeCanceled)
        {
            q = q.Where(s => !s.IsCanceled);
        }

        // Domyślnie ukryj minione sesje (user), admin może dodać includePast=true
        if (!includePast)
        {
            q = q.Where(s => s.EndAt > DateTime.UtcNow);
        }

        if (from.HasValue) q = q.Where(s => s.StartAt >= from.Value);
        if (to.HasValue) q = q.Where(s => s.StartAt <= to.Value);

        var items = await q
            .OrderBy(s => s.StartAt)
            .Select(s => new
            {
                s.ClassSessionId,
                s.ClassTypeId,
                ClassTypeName = s.ClassType.Name,
                s.StartAt,
                s.EndAt,
                s.Capacity,
                s.IsCanceled,
                ReservedCount = _db.ClassReservations.Count(r => r.ClassSessionId == s.ClassSessionId && r.Status == "reserved"),
                ReservedByMe = _db.ClassReservations.Any(r => r.ClassSessionId == s.ClassSessionId && r.UserId == userId && r.Status == "reserved")
            })
            .ToListAsync();

        var mapped = items.Select(x => new ClassSessionDto
        {
            ClassSessionId = x.ClassSessionId,
            ClassTypeId = x.ClassTypeId,
            ClassTypeName = x.ClassTypeName,
            StartAt = x.StartAt,
            EndAt = x.EndAt,
            Capacity = x.Capacity,
            IsCanceled = x.IsCanceled,
            ReservedCount = x.ReservedCount,
            Remaining = Math.Max(0, x.Capacity - x.ReservedCount),
            ReservedByMe = x.ReservedByMe
        }).ToList();

        return Ok(mapped);
    }

    // Reserve (user)
    [Authorize]
    [HttpPost("sessions/{sessionId:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid sessionId)
    {
        var userId = GetUserIdOrThrow();

        var session = await _db.ClassSessions.FirstOrDefaultAsync(s => s.ClassSessionId == sessionId);
        if (session is null) return NotFound();
        if (session.IsCanceled) return BadRequest("Zajęcia odwołane.");
        if (session.EndAt <= DateTime.UtcNow) return BadRequest("Zajęcia już się odbyły.");

        var reservedCount = await _db.ClassReservations.CountAsync(r => r.ClassSessionId == sessionId && r.Status == "reserved");
        if (reservedCount >= session.Capacity) return Conflict("Brak wolnych miejsc.");

        var existing = await _db.ClassReservations.FirstOrDefaultAsync(r => r.ClassSessionId == sessionId && r.UserId == userId);
        if (existing is not null)
        {
            if (existing.Status == "reserved") return Conflict("Już zapisano.");
            existing.Status = "reserved";
            existing.CanceledAt = null;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        _db.ClassReservations.Add(new ClassReservation
        {
            UserId = userId,
            ClassSessionId = sessionId,
            Status = "reserved",
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Nie udało się zapisać (spróbuj ponownie).");
        }

        return NoContent();
    }

    // Cancel (user)
    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}/reserve")]
    public async Task<IActionResult> Cancel(Guid sessionId)
    {
        var userId = GetUserIdOrThrow();

        var r = await _db.ClassReservations
            .FirstOrDefaultAsync(x => x.ClassSessionId == sessionId && x.UserId == userId);

        if (r is null) return NotFound();

        if (r.Status == "canceled") return NoContent();

        r.Status = "canceled";
        r.CanceledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Moje nadchodzące rezerwacje (user) — tylko przyszłe
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<ClassSessionDto>>> MyReservations()
    {
        var userId = GetUserIdOrThrow();

        var items = await _db.ClassReservations
            .Where(r => r.UserId == userId
                     && r.Status == "reserved"
                     && r.ClassSession.EndAt > DateTime.UtcNow
                     && !r.ClassSession.IsCanceled)
            .OrderBy(r => r.ClassSession.StartAt)
            .Select(r => new ClassSessionDto
            {
                ClassSessionId = r.ClassSessionId,
                ClassTypeId = r.ClassSession.ClassTypeId,
                ClassTypeName = r.ClassSession.ClassType.Name,
                StartAt = r.ClassSession.StartAt,
                EndAt = r.ClassSession.EndAt,
                Capacity = r.ClassSession.Capacity,
                IsCanceled = r.ClassSession.IsCanceled,
                ReservedCount = 0,
                Remaining = 0,
                ReservedByMe = true
            })
            .ToListAsync();

        return Ok(items);
    }

    // =====================
    // ADMIN: CRUD types
    // =====================

    [Authorize(Roles = "admin")]
    [HttpPost("types")]
    public async Task<IActionResult> CreateType(CreateClassTypeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 120) return BadRequest("Nieprawidłowa nazwa.");

        var type = new ClassType
        {
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            IsActive = true, // zawsze aktywny
            CreatedAt = DateTime.UtcNow
        };

        _db.ClassTypes.Add(type);
        await _db.SaveChangesAsync();
        return Ok(new { type.ClassTypeId });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("types/{id:guid}")]
    public async Task<IActionResult> UpdateType(Guid id, CreateClassTypeRequest req)
    {
        var type = await _db.ClassTypes.FirstOrDefaultAsync(x => x.ClassTypeId == id);
        if (type is null) return NotFound();

        type.Name = req.Name.Trim();
        type.Description = req.Description?.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Usunięcie typu zajęć — kaskadowo usuwa sesje i rezerwacje
    [Authorize(Roles = "admin")]
    [HttpDelete("types/{id:guid}")]
    public async Task<IActionResult> DeleteType(Guid id)
    {
        var type = await _db.ClassTypes.FirstOrDefaultAsync(x => x.ClassTypeId == id);
        if (type is null) return NotFound();

        // Pobierz ID sesji tego typu
        var sessionIds = await _db.ClassSessions
            .Where(s => s.ClassTypeId == id)
            .Select(s => s.ClassSessionId)
            .ToListAsync();

        if (sessionIds.Count > 0)
        {
            // Usuń rezerwacje powiązane z sesjami
            var reservations = await _db.ClassReservations
                .Where(r => sessionIds.Contains(r.ClassSessionId))
                .ToListAsync();
            _db.ClassReservations.RemoveRange(reservations);

            // Usuń sesje
            var sessions = await _db.ClassSessions
                .Where(s => s.ClassTypeId == id)
                .ToListAsync();
            _db.ClassSessions.RemoveRange(sessions);
        }

        _db.ClassTypes.Remove(type);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // =====================
    // ADMIN: CRUD sessions
    // =====================

    [Authorize(Roles = "admin")]
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(CreateClassSessionRequest req)
    {
        if (req.Capacity <= 0 || req.Capacity > 200) return BadRequest("Nieprawidłowa pojemność.");
        if (req.EndAt <= req.StartAt) return BadRequest("EndAt musi być > StartAt.");

        var typeExists = await _db.ClassTypes.AnyAsync(t => t.ClassTypeId == req.ClassTypeId);
        if (!typeExists) return BadRequest("Nie znaleziono typu zajęć.");

        var s = new ClassSession
        {
            ClassTypeId = req.ClassTypeId,
            StartAt = req.StartAt,
            EndAt = req.EndAt,
            Capacity = req.Capacity,
            IsCanceled = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.ClassSessions.Add(s);
        await _db.SaveChangesAsync();
        return Ok(new { s.ClassSessionId });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("sessions/{id:guid}")]
    public async Task<IActionResult> UpdateSession(Guid id, CreateClassSessionRequest req)
    {
        var s = await _db.ClassSessions.FirstOrDefaultAsync(x => x.ClassSessionId == id);
        if (s is null) return NotFound();

        if (req.EndAt <= req.StartAt) return BadRequest("EndAt musi być > StartAt.");
        if (req.Capacity <= 0 || req.Capacity > 200) return BadRequest("Nieprawidłowa pojemność.");

        s.ClassTypeId = req.ClassTypeId;
        s.StartAt = req.StartAt;
        s.EndAt = req.EndAt;
        s.Capacity = req.Capacity;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("sessions/{id:guid}/cancel")]
    public async Task<IActionResult> CancelSession(Guid id, [FromQuery] bool canceled)
    {
        var s = await _db.ClassSessions.FirstOrDefaultAsync(x => x.ClassSessionId == id);
        if (s is null) return NotFound();

        s.IsCanceled = canceled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Usunięcie sesji — kaskadowo usuwa rezerwacje
    [Authorize(Roles = "admin")]
    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var s = await _db.ClassSessions.FirstOrDefaultAsync(x => x.ClassSessionId == id);
        if (s is null) return NotFound();

        // Najpierw usuń powiązane rezerwacje (FK constraint)
        var reservations = await _db.ClassReservations
            .Where(r => r.ClassSessionId == id)
            .ToListAsync();
        _db.ClassReservations.RemoveRange(reservations);

        _db.ClassSessions.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Wypisanie użytkownika z zajęć (admin)
    [Authorize(Roles = "admin")]
    [HttpDelete("sessions/{sessionId:guid}/reservations/{userId:guid}")]
    public async Task<IActionResult> AdminRemoveReservation(Guid sessionId, Guid userId)
    {
        var r = await _db.ClassReservations
            .FirstOrDefaultAsync(x => x.ClassSessionId == sessionId && x.UserId == userId);

        if (r is null) return NotFound();

        _db.ClassReservations.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Lista obecności (admin)
    [Authorize(Roles = "admin")]
    [HttpGet("sessions/{id:guid}/attendance")]
    public async Task<ActionResult<List<AttendanceItem>>> Attendance(Guid id)
    {
        var exists = await _db.ClassSessions.AnyAsync(s => s.ClassSessionId == id);
        if (!exists) return NotFound();

        var items = await _db.ClassReservations
            .Where(r => r.ClassSessionId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AttendanceItem
            {
                UserId = r.UserId,
                Email = r.User.Email,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
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