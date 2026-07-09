namespace GymManager.Api.Dtos.Classes;

public class CreateClassSessionRequest
{
    public Guid ClassTypeId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; }
}