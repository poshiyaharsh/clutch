using Clutch.Web.Domain;
using Clutch.Web.Models;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clutch.Web.Controllers;

[Route("hackathons")]
public class HackathonsController : Controller
{
    private readonly IHackathonService _hackathonService;

    public HackathonsController(IHackathonService hackathonService)
    {
        _hackathonService = hackathonService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var hackathons = await _hackathonService.GetAllPublishedAsync();
        var vms = hackathons.Select(h => new HackathonListItemViewModel
        {
            Id = h.Id,
            Title = h.Title,
            Slug = h.Slug,
            Description = h.Description,
            BannerUrl = h.BannerUrl,
            StartsAt = h.StartsAt,
            EndsAt = h.EndsAt,
            RegistrationDeadline = h.RegistrationDeadline,
            SubmissionDeadline = h.SubmissionDeadline,
            Status = h.Status,
            TeamCount = h.Teams.Count,
            MaxTeamSize = h.MaxTeamSize
        }).ToList();

        return View(vms);
    }

    [HttpGet("create")]
    [Authorize(Policy = "RequireOrganizer")]
    public IActionResult Create()
    {
        return View(new HackathonCreateEditViewModel());
    }

    [HttpPost("create")]
    [Authorize(Policy = "RequireOrganizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HackathonCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _hackathonService.GetBySlugAsync(model.Slug);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(model.Slug), "This URL slug is already taken. Please choose another.");
            return View(model);
        }

        var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tracks = (model.TracksCommaSeparated ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var hackathon = new Hackathon
        {
            Title = model.Title.Trim(),
            Slug = model.Slug.Trim().ToLowerInvariant(),
            Description = model.Description.Trim(),
            BannerUrl = model.BannerUrl?.Trim(),
            StartsAt = model.StartsAt,
            EndsAt = model.EndsAt,
            RegistrationDeadline = model.RegistrationDeadline,
            SubmissionDeadline = model.SubmissionDeadline,
            MaxTeamSize = model.MaxTeamSize,
            HideLeaderboardUntilFinal = model.HideLeaderboardUntilFinal,
            Status = HackathonStatus.Published // Publish immediately on creation
        };

        await _hackathonService.CreateWithTracksAsync(hackathon, tracks, organizerId);
        TempData["ToastMessage"] = "Hackathon published successfully!";

        return RedirectToAction(nameof(Detail), new { slug = hackathon.Slug });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var h = await _hackathonService.GetDetailBySlugAsync(slug);
        if (h is null)
        {
            return NotFound();
        }

        var vm = new HackathonDetailViewModel
        {
            Id = h.Id,
            Title = h.Title,
            Slug = h.Slug,
            Description = h.Description,
            BannerUrl = h.BannerUrl,
            StartsAt = h.StartsAt,
            EndsAt = h.EndsAt,
            RegistrationDeadline = h.RegistrationDeadline,
            SubmissionDeadline = h.SubmissionDeadline,
            MaxTeamSize = h.MaxTeamSize,
            Status = h.Status,
            HideLeaderboardUntilFinal = h.HideLeaderboardUntilFinal,
            OrganizerName = h.Organizer?.DisplayName ?? "Organizer",
            OrganizerId = h.OrganizerId,
            Tracks = h.Tracks.Select(t => new TrackViewModel { Id = t.Id, Name = t.Name, Description = t.Description }).ToList(),
            Criteria = h.Criteria.Select(c => new CriterionViewModel { Id = c.Id, Name = c.Name, MaxPoints = c.MaxPoints, WeightPercent = c.WeightPercent }).ToList(),
            Announcements = h.Announcements.Select(a => new AnnouncementViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                AuthorName = a.CreatedBy?.DisplayName ?? "Organizer",
                CreatedAt = a.CreatedAt
            }).ToList(),
            Teams = h.Teams.Select(t => new TeamListItemViewModel
            {
                Id = t.Id,
                Name = t.Name,
                LeadName = t.Lead?.DisplayName ?? "Lead",
                MemberCount = t.Members.Count,
                HasSubmitted = t.Submission != null
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "RequireOrganizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, HackathonStatus status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(Roles.Admin);

        var success = await _hackathonService.UpdateStatusAsync(id, status, userId, isAdmin);
        if (!success)
        {
            return Forbid();
        }

        var h = await _hackathonService.GetByIdAsync(id);
        TempData["ToastMessage"] = $"Hackathon status updated to {status}.";
        return RedirectToAction(nameof(Detail), new { slug = h?.Slug });
    }

    [HttpPost("{id:int}/announcements")]
    [Authorize(Policy = "RequireOrganizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnnouncement(int id, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            TempData["ToastError"] = "Announcement title and content cannot be empty.";
            var current = await _hackathonService.GetByIdAsync(id);
            return RedirectToAction(nameof(Detail), new { slug = current?.Slug });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _hackathonService.AddAnnouncementAsync(id, title.Trim(), body.Trim(), userId);

        var h = await _hackathonService.GetByIdAsync(id);
        TempData["ToastMessage"] = "Announcement posted.";
        return RedirectToAction(nameof(Detail), new { slug = h?.Slug });
    }
}
