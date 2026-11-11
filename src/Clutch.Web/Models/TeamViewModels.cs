using System.ComponentModel.DataAnnotations;

namespace Clutch.Web.Models;

public class TeamListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public bool HasSubmitted { get; set; }
}

public class TeamCreateViewModel
{
    public int HackathonId { get; set; }
    public string HackathonTitle { get; set; } = string.Empty;
    public string HackathonSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Team name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Team name must be between 3 and 100 characters.")]
    [Display(Name = "Team name")]
    public string Name { get; set; } = string.Empty;
}

public class TeamJoinViewModel
{
    public int HackathonId { get; set; }
    public string HackathonTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Join code is required.")]
    [StringLength(16, MinimumLength = 6, ErrorMessage = "Enter a valid join code.")]
    [Display(Name = "8-character Join code")]
    public string JoinCode { get; set; } = string.Empty;
}

public class TeamDetailViewModel
{
    public int Id { get; set; }
    public int HackathonId { get; set; }
    public string HackathonTitle { get; set; } = string.Empty;
    public string HackathonSlug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public string LeadId { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public int MaxTeamSize { get; set; }
    public bool IsCurrentUserLead { get; set; }
    public bool IsCurrentUserMember { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public bool IsSubmissionOpen { get; set; }

    public List<TeamMemberViewModel> Members { get; set; } = [];
    public SubmissionViewModel? Submission { get; set; }
}

public class TeamMemberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CollegeOrCompany { get; set; }
    public bool IsLead { get; set; }
    public DateTime JoinedAt { get; set; }
}
