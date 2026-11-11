using Clutch.Web.Domain;
using System.ComponentModel.DataAnnotations;

namespace Clutch.Web.Models;

public class HackathonListItemViewModel
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
    public HackathonStatus Status { get; set; }
    public int TeamCount { get; set; }
    public int MaxTeamSize { get; set; }
}

public class HackathonDetailViewModel
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
    public int MaxTeamSize { get; set; }
    public HackathonStatus Status { get; set; }
    public bool HideLeaderboardUntilFinal { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerId { get; set; } = string.Empty;

    public List<TrackViewModel> Tracks { get; set; } = [];
    public List<CriterionViewModel> Criteria { get; set; } = [];
    public List<AnnouncementViewModel> Announcements { get; set; } = [];
    public List<TeamListItemViewModel> Teams { get; set; } = [];

    public bool IsRegistrationOpen => Status == HackathonStatus.Published && DateTime.UtcNow <= RegistrationDeadline;
    public bool IsSubmissionOpen => Status == HackathonStatus.Published && DateTime.UtcNow <= SubmissionDeadline;
}

public class HackathonCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required.")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
    [StringLength(220)]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Url(ErrorMessage = "Banner must be a valid URL.")]
    [Display(Name = "Banner image URL")]
    public string? BannerUrl { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Display(Name = "Starts at (UTC)")]
    public DateTime StartsAt { get; set; } = DateTime.UtcNow.AddDays(7);

    [Required(ErrorMessage = "End date is required.")]
    [Display(Name = "Ends at (UTC)")]
    public DateTime EndsAt { get; set; } = DateTime.UtcNow.AddDays(9);

    [Required(ErrorMessage = "Registration deadline is required.")]
    [Display(Name = "Registration deadline (UTC)")]
    public DateTime RegistrationDeadline { get; set; } = DateTime.UtcNow.AddDays(6);

    [Required(ErrorMessage = "Submission deadline is required.")]
    [Display(Name = "Submission deadline (UTC)")]
    public DateTime SubmissionDeadline { get; set; } = DateTime.UtcNow.AddDays(9);

    [Range(1, 10, ErrorMessage = "Team size must be between 1 and 10.")]
    [Display(Name = "Maximum team size")]
    public int MaxTeamSize { get; set; } = 4;

    [Display(Name = "Hide leaderboard until finalized")]
    public bool HideLeaderboardUntilFinal { get; set; }

    public string? TracksCommaSeparated { get; set; }
}

public class TrackViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CriterionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPoints { get; set; }
    public int WeightPercent { get; set; }
}

public class AnnouncementViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
