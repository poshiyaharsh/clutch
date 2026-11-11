using Microsoft.AspNetCore.Identity;

namespace Clutch.Web.Domain;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CollegeOrCompany { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Hackathon> OrganizedHackathons { get; set; } = [];
    public ICollection<Team> LedTeams { get; set; } = [];
    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
    public ICollection<JudgeAssignment> JudgeAssignments { get; set; } = [];
    public ICollection<Score> GivenScores { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
