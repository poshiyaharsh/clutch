using System.ComponentModel.DataAnnotations;

namespace Clutch.Web.Models;

public class JudgeHackathonListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TeamsCount { get; set; }
    public bool IsLocked { get; set; }
    public DateTime EndsAt { get; set; }
}

public class JudgeTeamScoreCardViewModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TrackName { get; set; }
    public string? ProjectTitle { get; set; }
    public bool HasSubmission { get; set; }
    public bool HasJudgeScored { get; set; }
    public bool CanJudge { get; set; }
}

public class RubricScoreFormViewModel
{
    public int HackathonId { get; set; }
    public string HackathonTitle { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TrackName { get; set; }
    public string? ProjectTitle { get; set; }
    public string? ProjectDescription { get; set; }
    public string? RepoUrl { get; set; }
    public string? DemoUrl { get; set; }
    public bool IsLocked { get; set; }

    public List<CriterionScoreInputViewModel> Criteria { get; set; } = [];
}

public class CriterionScoreInputViewModel
{
    public int CriterionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPoints { get; set; }
    public int WeightPercent { get; set; }

    [Range(0, 100)]
    public int Points { get; set; }
    public string? Comment { get; set; }
}

public class LeaderboardViewModel
{
    public int HackathonId { get; set; }
    public string HackathonTitle { get; set; } = string.Empty;
    public string HackathonSlug { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public bool HideUntilFinal { get; set; }
    public List<LeaderboardRowViewModel> Rows { get; set; } = [];
}

public class LeaderboardRowViewModel
{
    public int Rank { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TrackName { get; set; }
    public decimal TotalScore { get; set; }
    public decimal FirstCriterionScore { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
