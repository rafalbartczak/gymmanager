namespace GymManager.Api.Domain.Entities;

public class Entry
{
    public Guid EntryId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// Sposób wejścia: "qr_scan" (user skanuje kod klubu), "admin_scan" (admin skanuje kod usera), "manual" (admin ręcznie)
    /// </summary>
    public string EntryMethod { get; set; } = "manual";

    /// <summary>
    /// ID karnetu użytego przy wejściu (opcjonalne - może być null jeśli admin wpuścił bez karnetu)
    /// </summary>
    public Guid? PassId { get; set; }

    /// <summary>
    /// ID admina który zarejestrował wejście (dla manual/admin_scan)
    /// </summary>
    public Guid? RegisteredByAdminId { get; set; }

    public DateTime EntryAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public Pass? Pass { get; set; }
    public User? RegisteredByAdmin { get; set; }
}