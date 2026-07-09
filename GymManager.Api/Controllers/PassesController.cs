using GymManager.Api.Data;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Dtos.Passes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymManager.Api.Controllers;

[ApiController]
[Route("passes")]
public class PassesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PassesController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<MyPassItem>>> GetMine()
    {
        var userId = GetUserIdOrThrow();

        var items = await _db.Passes
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new MyPassItem
            {
                PassId = p.PassId,
                PassTypeName = p.PassType.Name,
                StartAt = p.StartAt,
                EndAt = p.EndAt,
                Status = p.Status,
                Price = p.PassType.Price,
                Currency = p.PassType.Currency
            })
            .ToListAsync();

        return Ok(items);
    }

    // Krok 1: Inicjalizacja płatności — tworzy Payment ze statusem "pending"
    [Authorize]
    [HttpPost("buy")]
    public async Task<ActionResult<BuyPassResponse>> Buy(BuyPassRequest req)
    {
        var userId = GetUserIdOrThrow();

        var passType = await _db.PassTypes
            .FirstOrDefaultAsync(p => p.PassTypeId == req.PassTypeId && p.IsActive);

        if (passType is null) return NotFound("Nie znaleziono typu karnetu.");

        // Sprawdź czy użytkownik ma już aktywny karnet
        var now = DateTime.UtcNow;
        var hasActivePass = await _db.Passes
            .AnyAsync(p => p.UserId == userId && p.Status == "active" && p.EndAt > now);

        if (hasActivePass)
            return Conflict("Posiadasz już aktywny karnet. Możesz kupić nowy dopiero po wygaśnięciu obecnego.");

        // Stwórz payment w stanie "pending" — symulacja przekierowania do bramki
        var payment = new Payment
        {
            UserId = userId,
            ProviderName = "MockPay",
            ProviderOrderId = Guid.NewGuid().ToString("N"),
            Status = "pending",
            Amount = passType.Price,
            Currency = passType.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return Ok(new BuyPassResponse
        {
            PaymentId = payment.PaymentId,
            PassTypeId = passType.PassTypeId,
            PassTypeName = passType.Name,
            Amount = passType.Price,
            Currency = passType.Currency,
            PaymentStatus = payment.Status,
            ProviderName = payment.ProviderName,
            ProviderOrderId = payment.ProviderOrderId
        });
    }

    // Krok 2: Potwierdzenie płatności — symulacja callbacku z bramki
    [Authorize]
    [HttpPost("confirm")]
    public async Task<ActionResult<ConfirmPaymentResponse>> ConfirmPayment(ConfirmPaymentRequest req)
    {
        var userId = GetUserIdOrThrow();

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == req.PaymentId && p.UserId == userId);

        if (payment is null) return NotFound("Nie znaleziono płatności.");

        if (payment.Status != "pending")
            return BadRequest("Płatność została już przetworzona.");

        var passType = await _db.PassTypes
            .FirstOrDefaultAsync(p => p.PassTypeId == req.PassTypeId && p.IsActive);

        if (passType is null) return NotFound("Nie znaleziono typu karnetu.");

        // Symulacja — bramka potwierdza płatność
        payment.Status = "completed";
        payment.CompletedAt = DateTime.UtcNow;

        // Aktywuj karnet
        var start = DateTime.UtcNow;
        var end = start.AddDays(passType.DurationDays);

        var pass = new Pass
        {
            UserId = userId,
            PassTypeId = passType.PassTypeId,
            StartAt = start,
            EndAt = end,
            Status = "active",
            PaymentId = payment.PaymentId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Passes.Add(pass);
        await _db.SaveChangesAsync();

        return Ok(new ConfirmPaymentResponse
        {
            PassId = pass.PassId,
            StartAt = pass.StartAt,
            EndAt = pass.EndAt,
            PaymentStatus = payment.Status
        });
    }

    private Guid GetUserIdOrThrow()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Brak poprawnego userId w tokenie.");

        return userId;
    }
}