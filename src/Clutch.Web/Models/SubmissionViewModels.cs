using System.ComponentModel.DataAnnotations;

namespace Clutch.Web.Models;

public class SubmissionViewModel
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? TrackId { get; set; }
    public string? TrackName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RepoUrl { get; set; } = string.Empty;
    public string? DemoUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SubmissionEditViewModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int HackathonId { get; set; }
    public string HackathonSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Project title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Summary & problem statement")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repository URL is required.")]
    [Url(ErrorMessage = "Enter a valid code repository URL (GitHub/GitLab).")]
    [Display(Name = "Repository URL")]
    public string RepoUrl { get; set; } = string.Empty;

    [Url(ErrorMessage = "Enter a valid demo or video URL.")]
    [Display(Name = "Live demo or video link (optional)")]
    public string? DemoUrl { get; set; }

    [Display(Name = "Competition track")]
    public int? TrackId { get; set; }

    public List<TrackViewModel> AvailableTracks { get; set; } = [];
}
