using GymManager.Api.Data;
using GymManager.Api.Dtos.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("admin/users")]
[Authorize(Roles = "admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminUsersController(AppDbContext db) => _db = db;

    // GET /admin/users?search=...
    [HttpGet]
    public async Task<ActionResult<List<AdminUserListItem>>> List([FromQuery] string? search)
    {
        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u => u.Email.ToLower().Contains(s));
        }

        var items = await q
            .OrderBy(u => u.Email)
            .Select(u => new AdminUserListItem
            {
                UserId = u.UserId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt
            })
            .Take(200)
            .ToListAsync();

        return Ok(items);
    }

    // GET /admin/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailsDto>> Details(Guid id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (u is null) return NotFound();

        // aktywny karnet (najprościej: EndAt > now i status active)
        var now = DateTime.UtcNow;
        var activePass = await _db.Passes
            .Where(p => p.UserId == id && p.Status == "active" && p.EndAt > now)
            .OrderByDescending(p => p.EndAt)
            .Select(p => new AdminUserPassInfo
            {
                PassId = p.PassId,
                PassTypeName = p.PassType.Name,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                Status = p.Status
            })
            .FirstOrDefaultAsync();

        // ostatnie rezerwacje zajęć
        var reservations = await _db.ClassReservations
            .Where(r => r.UserId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new AdminUserReservationInfo
            {
                ClassSessionId = r.ClassSessionId,
                ClassTypeName = r.ClassSession.ClassType.Name,
                StartAt = r.ClassSession.StartAt,
                EndAt = r.ClassSession.EndAt,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var dto = new AdminUserDetailsDto
        {
            UserId = u.UserId,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            IsActive = u.IsActive,
            IsDeleted = u.IsDeleted,
            CreatedAt = u.CreatedAt,
            ActivePass = activePass,
            RecentReservations = reservations
        };

        return Ok(dto);
    }
}