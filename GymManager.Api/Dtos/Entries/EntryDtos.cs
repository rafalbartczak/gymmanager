namespace GymManager.Api.Dtos.Entries;

public class EntryDto
{
    public Guid EntryId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = default!;
    public string EntryMethod { get; set; } = default!;
    public Guid? PassId { get; set; }
    public string? PassTypeName { get; set; }
    public DateTime EntryAt { get; set; }
    public Guid? RegisteredByAdminId { get; set; }
}

public class CheckInRequest
{
    public string ClubCode { get; set; } = default!;
}

public class VerifyEntryRequest
{
    public string UserCode { get; set; } = default!;
}

public class VerifyEntryResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string Message { get; set; } = default!;
    public string? UserEmail { get; set; }
    public Guid? UserId { get; set; }
    public string? PassTypeName { get; set; }
    public DateTime? PassValidUntil { get; set; }
    public Guid? EntryId { get; set; }
}

public class ManualEntryRequest
{
    public Guid UserId { get; set; }
}