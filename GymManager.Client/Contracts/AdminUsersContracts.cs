namespace GymManager.Client.Contracts;

public record AdminUserListItem(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAt
);

public record AdminUserPassInfo(
    Guid PassId,
    string PassTypeName,
    DateTime StartAt,
    DateTime EndAt,
    string Status
);

public record AdminUserReservationInfo(
    Guid ClassSessionId,
    string ClassTypeName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    DateTime CreatedAt
);

public record AdminUserDetailsDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAt,
    AdminUserPassInfo? ActivePass,
    List<AdminUserReservationInfo> RecentReservations
);