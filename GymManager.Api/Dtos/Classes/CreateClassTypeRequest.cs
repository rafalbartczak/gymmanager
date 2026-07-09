namespace GymManager.Api.Dtos.Classes;

public class CreateClassTypeRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}