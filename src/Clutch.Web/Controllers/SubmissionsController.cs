using Clutch.Web.Models;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clutch.Web.Controllers;

[Authorize]
[Route("submissions")]
public class SubmissionsController : Controller
{
    private readonly ITeamService _teamService;
    private readonly IHackathonService _hackathonService;

    public SubmissionsController(ITeamService teamService, IHackathonService hackathonService)
    {
        _teamService = teamService;
        _hackathonService = hackathonService;
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit(int teamId)
    {
        var team = await _teamService.GetByIdAsync(teamId);
        if (team is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (team.LeadId != userId)
        {
            return Forbid();
        }

        var hackathon = await _hackathonService.GetByIdAsync(team.HackathonId);
        if (hackathon is null) return NotFound();

        var vm = new SubmissionEditViewModel
        {
            TeamId = team.Id,
            TeamName = team.Name,
            HackathonId = hackathon.Id,
            HackathonSlug = hackathon.Slug,
            Title = team.Submission?.Title ?? string.Empty,
            Description = team.Submission?.Description ?? string.Empty,
            RepoUrl = team.Submission?.RepoUrl ?? string.Empty,
            DemoUrl = team.Submission?.DemoUrl,
            TrackId = team.Submission?.TrackId,
            AvailableTracks = hackathon.Tracks.Select(t => new TrackViewModel { Id = t.Id, Name = t.Name }).ToList()
        };

        return View(vm);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SubmissionEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var hackathon = await _hackathonService.GetByIdAsync(model.HackathonId);
            if (hackathon != null)
            {
                model.AvailableTracks = hackathon.Tracks.Select(t => new TrackViewModel { Id = t.Id, Name = t.Name }).ToList();
            }
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var submission = await _teamService.SaveSubmissionAsync(
            model.TeamId,
            model.Title,
            model.Description,
            model.RepoUrl,
            model.DemoUrl,
            model.TrackId,
            userId);

        if (submission is null)
        {
            ModelState.AddModelError(string.Empty, "Unable to save submission. Ensure you are squad lead and submission deadline has not passed.");
            var hackathon = await _hackathonService.GetByIdAsync(model.HackathonId);
            if (hackathon != null)
            {
                model.AvailableTracks = hackathon.Tracks.Select(t => new TrackViewModel { Id = t.Id, Name = t.Name }).ToList();
            }
            return View(model);
        }

        TempData["ToastMessage"] = "Project submission saved successfully!";
        return RedirectToAction("Detail", "Teams", new { id = model.TeamId });
    }
}
