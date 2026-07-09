using GymManager.Api.Data;
using GymManager.Api.Dtos.Passes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("passtypes")]
public class PassTypesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PassTypesController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<PassTypeListItem>>> GetActive()
    {
        var items = await _db.PassTypes
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => new PassTypeListItem
            {
                PassTypeId = p.PassTypeId,
                Name = p.Name,
                Description = p.Description,
                DurationDays = p.DurationDays,
                Price = p.Price,
                Currency = p.Currency
            })
            .ToListAsync();

        return Ok(items);
    }
}