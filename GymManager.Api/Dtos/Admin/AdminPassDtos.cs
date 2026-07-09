namespace GymManager.Api.Dtos.Admin;

public class AdminAssignPassRequest
{
    public Guid UserId { get; set; }
    public Guid PassTypeId { get; set; }
}

public class AdminCancelPassRequest
{
    public Guid PassId { get; set; }
}