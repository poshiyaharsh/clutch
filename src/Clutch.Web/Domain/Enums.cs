namespace Clutch.Web.Domain;

public enum HackathonStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2,
    Finalized = 3,
    Cancelled = 4
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string Participant = "Participant";
    public const string Judge = "Judge";

    public static readonly string[] All = [Admin, Organizer, Participant, Judge];
}
