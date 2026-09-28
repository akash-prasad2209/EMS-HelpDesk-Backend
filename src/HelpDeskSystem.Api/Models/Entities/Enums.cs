namespace HelpDeskSystem.Api.Models.Entities;

public enum TicketImpact
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum TicketUrgency
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum TicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TicketStatus
{
    New = 1,
    Assigned = 2,
    InProgress = 3,
    Resolved = 4,
    Closed = 5,
    Reopened = 6
}

// Well-known role names. Seeded into the Role table; also used as string
// constants for [Authorize(Roles = ...)] so nothing is a magic string twice.
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string Hr = "HR";
    public const string It = "IT";
}

// Well-known department names, matching the seeded Department rows.
public static class DepartmentNames
{
    public const string It = "IT";
    public const string Hr = "HR";
    public const string Admin = "Admin";
}
