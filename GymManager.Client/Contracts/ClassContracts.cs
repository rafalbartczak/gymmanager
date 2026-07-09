namespace GymManager.Client.Contracts;

public record ClassTypeDto(
    Guid ClassTypeId,
    string Name,
    string? Description,
    bool IsActive
);

public record CreateClassTypeRequest(
    string Name,
    string? Description
);

public record ClassSessionDto(
    Guid ClassSessionId,
    Guid ClassTypeId,
    string ClassTypeName,
    DateTime StartAt,
    DateTime EndAt,
    int Capacity,
    bool IsCanceled,
    int ReservedCount,
    int Remaining,
    bool ReservedByMe
);

public record CreateClassSessionRequest(
    Guid ClassTypeId,
    DateTime StartAt,
    DateTime EndAt,
    int Capacity
);

public record AttendanceItem(
    Guid UserId,
    string Email,
    string Status,
    DateTime CreatedAt
);