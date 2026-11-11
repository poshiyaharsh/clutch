using Clutch.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Clutch.Web.Controllers;

// Phase 3 preview controller. Split into Hackathons/Teams/Judging/Leaderboard/Organizer controllers in Phase 5–6.
public class HomeController : Controller
{
    public IActionResult Index() => View(MockData.Events);

    [HttpGet("events/{id:int}")]
    public IActionResult Event(int id)
    {
        var ev = MockData.Events.FirstOrDefault(e => e.Id == id);
        return ev is null ? NotFound() : View(ev);
    }

    [HttpGet("events/{id:int}/leaderboard")]
    public IActionResult Leaderboard(int id) => View(MockData.Leaderboard);

    [HttpGet("teams/preview")]
    public IActionResult Team() => View();

    [HttpGet("judge/preview")]
    public IActionResult Judge() => View(MockData.Rubric);

    [HttpGet("organizer/preview")]
    public IActionResult Organizer() => View();
}
