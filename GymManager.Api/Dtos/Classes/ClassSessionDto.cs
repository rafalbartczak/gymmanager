namespace GymManager.Api.Dtos.Classes;

public class ClassSessionDto
{
    public Guid ClassSessionId { get; set; }
    public Guid ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; }
    public bool IsCanceled { get; set; }

    public int ReservedCount { get; set; }
    public int Remaining { get; set; }
    public bool ReservedByMe { get; set; }
}