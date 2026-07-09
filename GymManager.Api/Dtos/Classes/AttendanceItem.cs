namespace GymManager.Api.Dtos.Classes;

public class AttendanceItem
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}