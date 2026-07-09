namespace GymManager.Api.Dtos.Classes;

public class ClassTypeDto
{
    public Guid ClassTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}