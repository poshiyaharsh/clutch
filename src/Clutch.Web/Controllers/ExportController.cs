using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Clutch.Web.Controllers;

[Authorize(Policy = "RequireOrganizer")]
[Route("export")]
public class ExportController : Controller
{
    private readonly IHackathonService _hackathonService;
    private readonly IScoringService _scoringService;
    private readonly ITeamService _teamService;

    public ExportController(
        IHackathonService hackathonService,
        IScoringService scoringService,
        ITeamService teamService)
    {
        _hackathonService = hackathonService;
        _scoringService = scoringService;
        _teamService = teamService;
    }

    [HttpGet("{hackathonId:int}/teams.csv")]
    public async Task<IActionResult> ExportTeamsCsv(int hackathonId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        if (hackathon is null) return NotFound();

        var teams = await _scoringService.GetTeamsForJudgingAsync(hackathonId);

        var sb = new StringBuilder();
        sb.AppendLine("TeamId,TeamName,JoinCode,LeaderName,LeaderEmail,MemberCount,Track,HasSubmission,ProjectTitle,RepoUrl,SubmittedAt");

        foreach (var t in teams)
        {
            var cleanTeamName = EscapeCsv(t.Name);
            var cleanLeadName = EscapeCsv(t.Lead?.DisplayName ?? "");
            var cleanLeadEmail = EscapeCsv(t.Lead?.Email ?? "");
            var cleanTrack = EscapeCsv(t.Submission?.Track?.Name ?? "General");
            var cleanProjectTitle = EscapeCsv(t.Submission?.Title ?? "");
            var cleanRepoUrl = EscapeCsv(t.Submission?.RepoUrl ?? "");
            var submittedAt = t.Submission?.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            sb.AppendLine($"{t.Id},{cleanTeamName},{t.JoinCode},{cleanLeadName},{cleanLeadEmail},{t.Members.Count},{cleanTrack},{t.Submission != null},{cleanProjectTitle},{cleanRepoUrl},{submittedAt}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"{hackathon.Slug}-teams-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet("{hackathonId:int}/scores.csv")]
    public async Task<IActionResult> ExportScoresCsv(int hackathonId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        if (hackathon is null) return NotFound();

        var results = await _scoringService.CalculateLeaderboardAsync(hackathonId);

        var sb = new StringBuilder();
        sb.AppendLine("Rank,TeamId,TeamName,Track,TotalScore,FirstCriterionScore,SubmittedAt");

        foreach (var r in results)
        {
            var cleanTeamName = EscapeCsv(r.TeamName);
            var cleanTrack = EscapeCsv(r.TrackName ?? "General");
            var submittedAt = r.SubmittedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            sb.AppendLine($"{r.Rank},{r.TeamId},{cleanTeamName},{cleanTrack},{r.TotalScore:F2},{r.FirstCriterionScore:F2},{submittedAt}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"{hackathon.Slug}-scoreboard-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
