using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public interface IHackathonService
{
    Task<IReadOnlyList<Hackathon>> GetAllPublishedAsync();
    Task<IReadOnlyList<Hackathon>> GetForOrganizerAsync(string organizerId);
    Task<Hackathon?> GetByIdAsync(int id);
    Task<Hackathon?> GetBySlugAsync(string slug);
    Task<Hackathon?> GetDetailBySlugAsync(string slug);
    Task<Hackathon> CreateAsync(Hackathon hackathon, string organizerId);
    Task<Hackathon> CreateWithTracksAsync(Hackathon hackathon, IEnumerable<string> tracks, string organizerId);
    Task<bool> UpdateStatusAsync(int id, HackathonStatus newStatus, string actorId, bool isAdmin = false);
    Task<Announcement> AddAnnouncementAsync(int hackathonId, string title, string body, string authorId);
}

public class HackathonService : IHackathonService
{
    private readonly Data.AppDbContext _db;

    public HackathonService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Hackathon>> GetAllPublishedAsync()
    {
        return await _db.Hackathons
            .AsNoTracking()
            .Where(h => h.Status == HackathonStatus.Published)
            .Include(h => h.Teams)
            .OrderByDescending(h => h.StartsAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hackathon>> GetForOrganizerAsync(string organizerId)
    {
        return await _db.Hackathons
            .AsNoTracking()
            .Where(h => h.OrganizerId == organizerId)
            .Include(h => h.Teams)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<Hackathon?> GetByIdAsync(int id)
    {
        return await _db.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Criteria)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Hackathon?> GetBySlugAsync(string slug)
    {
        return await _db.Hackathons.FirstOrDefaultAsync(h => h.Slug == slug);
    }

    public async Task<Hackathon?> GetDetailBySlugAsync(string slug)
    {
        return await _db.Hackathons
            .Include(h => h.Organizer)
            .Include(h => h.Tracks)
            .Include(h => h.Criteria.OrderBy(c => c.SortOrder))
            .Include(h => h.Announcements.OrderByDescending(a => a.CreatedAt)).ThenInclude(a => a.CreatedBy)
            .Include(h => h.Teams).ThenInclude(t => t.Lead)
            .Include(h => h.Teams).ThenInclude(t => t.Members)
            .Include(h => h.Teams).ThenInclude(t => t.Submission)
            .FirstOrDefaultAsync(h => h.Slug == slug);
    }

    public Task<Hackathon> CreateAsync(Hackathon hackathon, string organizerId)
    {
        return CreateWithTracksAsync(hackathon, [], organizerId);
    }

    public async Task<Hackathon> CreateWithTracksAsync(Hackathon hackathon, IEnumerable<string> tracks, string organizerId)
    {
        hackathon.OrganizerId = organizerId;
        hackathon.CreatedAt = DateTime.UtcNow;

        foreach (var trackName in tracks.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            hackathon.Tracks.Add(new Track { Name = trackName.Trim() });
        }

        _db.Hackathons.Add(hackathon);
        await _db.SaveChangesAsync();
        return hackathon;
    }

    public async Task<bool> UpdateStatusAsync(int id, HackathonStatus newStatus, string actorId, bool isAdmin = false)
    {
        var h = await _db.Hackathons.FindAsync(id);
        if (h is null) return false;
        if (!isAdmin && h.OrganizerId != actorId) return false;

        h.Status = newStatus;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Announcement> AddAnnouncementAsync(int hackathonId, string title, string body, string authorId)
    {
        var announcement = new Announcement
        {
            HackathonId = hackathonId,
            Title = title,
            Body = body,
            CreatedById = authorId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();
        return announcement;
    }
}
