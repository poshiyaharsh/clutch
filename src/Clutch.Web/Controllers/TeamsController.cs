using Clutch.Web.Domain;
using Clutch.Web.Models;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clutch.Web.Controllers;

[Authorize]
[Route("teams")]
public class TeamsController : Controller
{
    private readonly ITeamService _teamService;
    private readonly IHackathonService _hackathonService;
    private readonly IScoringService _scoringService;

    public TeamsController(
        ITeamService teamService,
        IHackathonService hackathonService,
        IScoringService scoringService)
    {
        _teamService = teamService;
        _hackathonService = hackathonService;
        _scoringService = scoringService;
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int hackathonId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        if (hackathon is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existingTeam = await _teamService.GetByHackathonAndUserAsync(hackathonId, userId);
        if (existingTeam is not null)
        {
            TempData["ToastError"] = "You already have a team in this hackathon.";
            return RedirectToAction(nameof(Detail), new { id = existingTeam.Id });
        }

        return View(new TeamCreateViewModel
        {
            HackathonId = hackathon.Id,
            HackathonTitle = hackathon.Title,
            HackathonSlug = hackathon.Slug
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            var team = await _teamService.CreateTeamAsync(model.HackathonId, model.Name, userId);
            TempData["ToastMessage"] = $"Squad '{team.Name}' created! Share your code {team.JoinCode} with teammates.";
            return RedirectToAction(nameof(Detail), new { id = team.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("join")]
    public async Task<IActionResult> Join(int hackathonId)
    {
        var hackathon = await _hackathonService.GetByIdAsync(hackathonId);
        if (hackathon is null) return NotFound();

        return View(new TeamJoinViewModel
        {
            HackathonId = hackathon.Id,
            HackathonTitle = hackathon.Title
        });
    }

    [HttpPost("join")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(TeamJoinViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _teamService.JoinTeamAsync(model.JoinCode.Trim().ToUpperInvariant(), userId);

        if (!success)
        {
            ModelState.AddModelError(nameof(model.JoinCode), "Unable to join squad. The code may be invalid, the team is full, or you are already in a team for this hackathon.");
            return View(model);
        }

        var team = await _teamService.GetByJoinCodeAsync(model.JoinCode.Trim().ToUpperInvariant());
        TempData["ToastMessage"] = "Successfully joined squad!";
        return RedirectToAction(nameof(Detail), new { id = team?.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var team = await _teamService.GetByIdAsync(id);
        if (team is null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isLead = team.LeadId == currentUserId;
        var isMember = team.Members.Any(m => m.UserId == currentUserId);

        var vm = new TeamDetailViewModel
        {
            Id = team.Id,
            HackathonId = team.HackathonId,
            HackathonTitle = team.Hackathon.Title,
            HackathonSlug = team.Hackathon.Slug,
            Name = team.Name,
            JoinCode = team.JoinCode,
            LeadId = team.LeadId,
            LeadName = team.Lead?.DisplayName ?? "Leader",
            MaxTeamSize = team.Hackathon.MaxTeamSize,
            IsCurrentUserLead = isLead,
            IsCurrentUserMember = isMember,
            IsRegistrationOpen = team.Hackathon.Status == HackathonStatus.Published && DateTime.UtcNow <= team.Hackathon.RegistrationDeadline,
            IsSubmissionOpen = team.Hackathon.Status == HackathonStatus.Published && DateTime.UtcNow <= team.Hackathon.SubmissionDeadline,
            Members = team.Members.Select(m => new TeamMemberViewModel
            {
                UserId = m.UserId,
                DisplayName = m.User?.DisplayName ?? "Member",
                CollegeOrCompany = m.User?.CollegeOrCompany,
                IsLead = m.UserId == team.LeadId,
                JoinedAt = m.JoinedAt
            }).ToList(),
            Submission = team.Submission == null ? null : new SubmissionViewModel
            {
                Id = team.Submission.Id,
                TeamId = team.Id,
                TeamName = team.Name,
                Title = team.Submission.Title,
                Description = team.Submission.Description,
                RepoUrl = team.Submission.RepoUrl,
                DemoUrl = team.Submission.DemoUrl,
                TrackId = team.Submission.TrackId,
                TrackName = team.Submission.Track?.Name,
                SubmittedAt = team.Submission.SubmittedAt,
                UpdatedAt = team.Submission.UpdatedAt
            }
        };

        return View(vm);
    }

    [HttpPost("{id:int}/leave")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _teamService.LeaveTeamAsync(id, userId);

        if (!success)
        {
            TempData["ToastError"] = "Could not leave squad. As team leader, you must transfer leadership to another member before leaving.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        TempData["ToastMessage"] = "You have left the squad.";
        return RedirectToAction("Index", "Hackathons");
    }

    [HttpPost("{id:int}/remove-member")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id, string memberUserId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _teamService.RemoveMemberAsync(id, memberUserId, currentUserId);

        if (!success)
        {
            TempData["ToastError"] = "Unable to remove team member.";
        }
        else
        {
            TempData["ToastMessage"] = "Member removed from squad.";
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("{id:int}/transfer-lead")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferLead(int id, string newLeadId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _teamService.TransferLeadAsync(id, currentUserId, newLeadId);

        if (!success)
        {
            TempData["ToastError"] = "Failed to transfer squad leadership.";
        }
        else
        {
            TempData["ToastMessage"] = "Squad leadership transferred successfully.";
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/card")]
    public async Task<IActionResult> ResultCard(int id)
    {
        var team = await _teamService.GetByIdAsync(id);
        if (team is null) return NotFound();

        var leaderboard = await _scoringService.CalculateLeaderboardAsync(team.HackathonId);
        var entry = leaderboard.FirstOrDefault(r => r.TeamId == id);

        ViewData["Rank"] = entry?.Rank ?? 0;
        ViewData["Score"] = entry?.TotalScore ?? 0m;
        ViewData["IsLocked"] = await _scoringService.IsJudgingLockedAsync(team.HackathonId);

        return View(team);
    }
}
