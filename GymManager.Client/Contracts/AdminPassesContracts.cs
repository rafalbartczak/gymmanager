namespace GymManager.Client.Contracts;

public record AdminAssignPassRequest(Guid UserId, Guid PassTypeId);
public record AdminCancelPassRequest(Guid PassId);