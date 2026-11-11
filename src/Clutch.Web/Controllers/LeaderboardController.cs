using Clutch.Web.Models;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clutch.Web.Controllers;

[Route("leaderboard")]
public class LeaderboardController : Controller
{
    private readonly IScoringService _scoringService;
    private readonly IHackathonService _hackathonService;

    public LeaderboardController(IScoringService scoringService, IHackathonService hackathonService)
    {
        _scoringService = scoringService;
        _hackathonService = hackathonService;
    }

    [HttpGet("{hackathonId:int}")]
    public async Task<IActionResult> Index(int hackathonId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        if (hackathon is null) return NotFound();

        var isLocked = await _scoringService.IsJudgingLockedAsync(hackathonId);
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOrganizerOrAdmin = User.IsInRole("Admin") || (currentUserId != null && hackathon.OrganizerId == currentUserId);

        if (hackathon.HideLeaderboardUntilFinal && !isLocked && !isOrganizerOrAdmin)
        {
            return View("Hidden", new LeaderboardViewModel
            {
                HackathonId = hackathon.Id,
                HackathonTitle = hackathon.Title,
                HackathonSlug = hackathon.Slug,
                IsLocked = false,
                HideUntilFinal = true
            });
        }

        var results = await _scoringService.CalculateLeaderboardAsync(hackathonId);
        var vm = new LeaderboardViewModel
        {
            HackathonId = hackathon.Id,
            HackathonTitle = hackathon.Title,
            HackathonSlug = hackathon.Slug,
            IsLocked = isLocked,
            HideUntilFinal = hackathon.HideLeaderboardUntilFinal,
            Rows = results.Select(r => new LeaderboardRowViewModel
            {
                Rank = r.Rank,
                TeamId = r.TeamId,
                TeamName = r.TeamName,
                TrackName = r.TrackName,
                TotalScore = r.TotalScore,
                FirstCriterionScore = r.FirstCriterionScore,
                SubmittedAt = r.SubmittedAt
            }).ToList()
        };

        return View(vm);
    }
}
