namespace Clutch.Web.Domain;

public class Hackathon
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public int MaxTeamSize { get; set; } = 4;
    public HackathonStatus Status { get; set; } = HackathonStatus.Draft;
    public bool HideLeaderboardUntilFinal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string OrganizerId { get; set; } = string.Empty;
    public ApplicationUser? Organizer { get; set; }

    public ICollection<Track> Tracks { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<Criterion> Criteria { get; set; } = [];
    public ICollection<JudgeAssignment> JudgeAssignments { get; set; } = [];
    public ICollection<Announcement> Announcements { get; set; } = [];
    public JudgingLock? JudgingLock { get; set; }
}

public class Track
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Submission> Submissions { get; set; } = [];
}

public class Team
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public string LeadId { get; set; } = string.Empty;
    public ApplicationUser? Lead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TeamMember> Members { get; set; } = [];
    public Submission? Submission { get; set; }
    public ICollection<Score> Scores { get; set; } = [];
}

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Submission
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public int? TrackId { get; set; }
    public Track? Track { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RepoUrl { get; set; } = string.Empty;
    public string? DemoUrl { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Criterion
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPoints { get; set; } = 10;
    public int WeightPercent { get; set; }
    public int SortOrder { get; set; }

    public ICollection<Score> Scores { get; set; } = [];
}

public class JudgeAssignment
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public string JudgeId { get; set; } = string.Empty;
    public ApplicationUser Judge { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public class Score
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public int CriterionId { get; set; }
    public Criterion Criterion { get; set; } = null!;
    public string JudgeId { get; set; } = string.Empty;
    public ApplicationUser Judge { get; set; } = null!;
    public int Points { get; set; }
    public string? Comment { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class JudgingLock
{
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
    public string LockedById { get; set; } = string.Empty;
    public ApplicationUser LockedBy { get; set; } = null!;
}

public class Announcement
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public Hackathon Hackathon { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public string? ActorId { get; set; }
    public ApplicationUser? Actor { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
