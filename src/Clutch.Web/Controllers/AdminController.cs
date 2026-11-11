using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Controllers;

[Authorize(Policy = "RequireAdmin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _auditService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        AppDbContext db,
        IAuditService auditService,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _auditService = auditService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var hackathonsCount = await _db.Hackathons.CountAsync();
        var usersCount = await _db.Users.CountAsync();
        var teamsCount = await _db.Teams.CountAsync();
        var judgesCount = await _db.UserRoles
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == Roles.Judge)
            .CountAsync();

        var recentLogs = await _auditService.GetRecentLogsAsync(20);
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).Take(20).ToListAsync();

        ViewData["HackathonsCount"] = hackathonsCount;
        ViewData["UsersCount"] = usersCount;
        ViewData["TeamsCount"] = teamsCount;
        ViewData["JudgesCount"] = judgesCount;
        ViewData["Users"] = users;

        return View(recentLogs);
    }

    [HttpGet("audit")]
    public async Task<IActionResult> Audit()
    {
        var logs = await _auditService.GetRecentLogsAsync(100);
        return View(logs);
    }
}
