namespace GymManager.Api.Dtos.Admin;

public class AdminUserListItem
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserDetailsDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public AdminUserPassInfo? ActivePass { get; set; }
    public List<AdminUserReservationInfo> RecentReservations { get; set; } = new();
}

public class AdminUserPassInfo
{
    public Guid PassId { get; set; }
    public string PassTypeName { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = default!;
}

public class AdminUserReservationInfo
{
    public Guid ClassSessionId { get; set; }
    public string ClassTypeName { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}