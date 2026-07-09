using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Announcements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("announcements")]
public class AnnouncementsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnnouncementsController(AppDbContext db)
    {
        _db = db;
    }

    // Dla klienta (zalogowany): lista opublikowanych
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<AnnouncementListItem>>> GetPublished()
    {
        var items = await _db.Announcements
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => new AnnouncementListItem
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                IsPublished = a.IsPublished,
                PublishedAt = a.PublishedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    // Dla admina: lista wszystkich (published + unpublished)
    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<List<AnnouncementListItem>>> GetAllForAdmin()
    {
        var items = await _db.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementListItem
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                IsPublished = a.IsPublished,
                PublishedAt = a.PublishedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    // Dla admina: create
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Length > 200)
            return BadRequest("Nieprawidłowy tytuł (1..200).");

        if (string.IsNullOrWhiteSpace(req.Content) || req.Content.Length > 4000)
            return BadRequest("Nieprawidłowa treść (1..4000).");

        var adminIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(adminIdStr) || !Guid.TryParse(adminIdStr, out var adminId))
            return Unauthorized();

        var entity = new Announcement
        {
            Title = req.Title.Trim(),
            Content = req.Content.Trim(),
            IsPublished = req.IsPublished,
            PublishedAt = req.IsPublished ? DateTime.UtcNow : DateTime.MinValue,
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Announcements.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPublished), new { id = entity.AnnouncementId }, new { entity.AnnouncementId });
    }

    // Dla admina: publish/unpublish (zmiana statusu)
    [Authorize(Roles = "admin")]
    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> SetPublish(Guid id, [FromQuery] bool published)
    {
        var entity = await _db.Announcements.FirstOrDefaultAsync(a => a.AnnouncementId == id);
        if (entity is null) return NotFound();

        entity.IsPublished = published;
        entity.PublishedAt = published ? DateTime.UtcNow : DateTime.MinValue;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Dla admina: delete
    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Announcements.FirstOrDefaultAsync(a => a.AnnouncementId == id);
        if (entity is null) return NotFound();

        _db.Announcements.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}