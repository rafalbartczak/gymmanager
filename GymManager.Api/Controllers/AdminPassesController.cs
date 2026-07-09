using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("admin/passes")]
[Authorize(Roles = "admin")]
public class AdminPassesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminPassesController(AppDbContext db) => _db = db;

    // POST /admin/passes/assign
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AdminAssignPassRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("Brak UserId.");
        if (req.PassTypeId == Guid.Empty) return BadRequest("Brak PassTypeId.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == req.UserId);
        if (user is null) return NotFound("Nie znaleziono użytkownika.");

        var passType = await _db.PassTypes.FirstOrDefaultAsync(p => p.PassTypeId == req.PassTypeId && p.IsActive);
        if (passType is null) return BadRequest("Nie znaleziono typu karnetu (lub nieaktywny).");

        var now = DateTime.UtcNow;

        // aktywny karnet (jeśli jest)
        var active = await _db.Passes
            .Where(p => p.UserId == req.UserId && p.Status == "active" && p.EndAt > now)
            .FirstOrDefaultAsync();

        if (active is not null)
            return Conflict("Użytkownik posiada już aktywny karnet. Najpierw anuluj obecny.");

        // brak aktywnego -> utwórz nowy
        var pass = new Pass
        {
            UserId = req.UserId,
            PassTypeId = req.PassTypeId,
            StartAt = now,
            EndAt = now.AddDays(passType.DurationDays),
            Status = "active",
            PaymentId = null,
            CreatedAt = now
        };

        _db.Passes.Add(pass);
        await _db.SaveChangesAsync();

        return Ok(new { mode = "created", passId = pass.PassId, endAt = pass.EndAt });
    }


    // POST /admin/passes/cancel
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(AdminCancelPassRequest req)
    {
        if (req.PassId == Guid.Empty) return BadRequest("Brak PassId.");

        var pass = await _db.Passes.FirstOrDefaultAsync(p => p.PassId == req.PassId);
        if (pass is null) return NotFound();

        if (pass.Status == "canceled") return NoContent();

        pass.Status = "canceled";
        pass.EndAt = DateTime.UtcNow; // natychmiastowe unieważnienie

        await _db.SaveChangesAsync();
        return NoContent();
    }
}