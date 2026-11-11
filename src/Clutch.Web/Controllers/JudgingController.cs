using Clutch.Web.Domain;
using Clutch.Web.Models;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clutch.Web.Controllers;

[Authorize(Policy = "RequireJudge")]
[Route("judging")]
public class JudgingController : Controller
{
    private readonly IScoringService _scoringService;
    private readonly IHackathonService _hackathonService;
    private readonly ITeamService _teamService;

    public JudgingController(
        IScoringService scoringService,
        IHackathonService hackathonService,
        ITeamService teamService)
    {
        _scoringService = scoringService;
        _hackathonService = hackathonService;
        _teamService = teamService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var judgeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var hackathons = await _scoringService.GetAssignedHackathonsForJudgeAsync(judgeId);

        var vms = hackathons.Select(h => new JudgeHackathonListItemViewModel
        {
            Id = h.Id,
            Title = h.Title,
            Slug = h.Slug,
            TeamsCount = h.Teams.Count,
            IsLocked = h.JudgingLock != null,
            EndsAt = h.EndsAt
        }).ToList();

        return View(vms);
    }

    [HttpGet("hackathon/{id:int}")]
    public async Task<IActionResult> HackathonTeams(int id)
    {
        var hackathon = await _hackathonService.GetByIdAsync(id);
        if (hackathon is null) return NotFound();

        var judgeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var teams = await _scoringService.GetTeamsForJudgingAsync(id);

        var teamCards = new List<JudgeTeamScoreCardViewModel>();
        foreach (var t in teams)
        {
            var canJudge = await _scoringService.CanJudgeTeamAsync(t.Id, judgeId);
            var scores = await _scoringService.GetTeamScoresForJudgeAsync(t.Id, judgeId);

            teamCards.Add(new JudgeTeamScoreCardViewModel
            {
                TeamId = t.Id,
                TeamName = t.Name,
                TrackName = t.Submission?.Track?.Name,
                ProjectTitle = t.Submission?.Title,
                HasSubmission = t.Submission != null,
                HasJudgeScored = scores.Count > 0,
                CanJudge = canJudge
            });
        }

        ViewData["HackathonTitle"] = hackathon.Title;
        ViewData["HackathonId"] = hackathon.Id;
        ViewData["IsLocked"] = await _scoringService.IsJudgingLockedAsync(id);

        return View(teamCards);
    }

    [HttpGet("hackathon/{hackathonId:int}/team/{teamId:int}")]
    public async Task<IActionResult> ScoreTeam(int hackathonId, int teamId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        var team = await _teamService.GetByIdAsync(teamId);
        if (hackathon is null || team is null) return NotFound();

        var judgeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var canJudge = await _scoringService.CanJudgeTeamAsync(teamId, judgeId);
        if (!canJudge)
        {
            TempData["ToastError"] = "You cannot score this squad (judging is locked or you are a squad member).";
            return RedirectToAction(nameof(HackathonTeams), new { id = hackathonId });
        }

        var criteria = await _scoringService.GetCriteriaAsync(hackathonId);
        var existingScores = await _scoringService.GetTeamScoresForJudgeAsync(teamId, judgeId);
        var isLocked = await _scoringService.IsJudgingLockedAsync(hackathonId);

        var vm = new RubricScoreFormViewModel
        {
            HackathonId = hackathon.Id,
            HackathonTitle = hackathon.Title,
            TeamId = team.Id,
            TeamName = team.Name,
            TrackName = team.Submission?.Track?.Name,
            ProjectTitle = team.Submission?.Title,
            ProjectDescription = team.Submission?.Description,
            RepoUrl = team.Submission?.RepoUrl,
            DemoUrl = team.Submission?.DemoUrl,
            IsLocked = isLocked,
            Criteria = criteria.Select(c =>
            {
                var s = existingScores.FirstOrDefault(score => score.CriterionId == c.Id);
                return new CriterionScoreInputViewModel
                {
                    CriterionId = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    MaxPoints = c.MaxPoints,
                    WeightPercent = c.WeightPercent,
                    Points = s?.Points ?? 0,
                    Comment = s?.Comment
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost("hackathon/{hackathonId:int}/team/{teamId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScoreTeam(int hackathonId, int teamId, RubricScoreFormViewModel model)
    {
        var judgeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            foreach (var c in model.Criteria)
            {
                await _scoringService.SubmitScoreAsync(teamId, c.CriterionId, judgeId, c.Points, c.Comment);
            }

            TempData["ToastMessage"] = $"Scores saved for squad '{model.TeamName}'.";
            return RedirectToAction(nameof(HackathonTeams), new { id = hackathonId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost("hackathon/{id:int}/lock")]
    [Authorize(Policy = "RequireOrganizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockJudging(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _scoringService.LockJudgingAsync(id, userId);

        TempData["ToastMessage"] = "Judging has been finalized and locked.";
        return RedirectToAction(nameof(HackathonTeams), new { id });
    }
}
