using Clutch.Web.Domain;

namespace Clutch.Web.Services;

public interface IHackathonService
{
    Task<IReadOnlyList<Hackathon>> GetAllPublishedAsync();
    Task<Hackathon?> GetByIdAsync(int id);
    Task<Hackathon?> GetBySlugAsync(string slug);
    Task<Hackathon> CreateAsync(Hackathon hackathon, string organizerId);
    Task UpdateStatusAsync(int id, HackathonStatus newStatus, string actorId);
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
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(
                System.Linq.Queryable.Where(_db.Hackathons, h => h.Status == HackathonStatus.Published)
            )
        );
    }

    public async Task<Hackathon?> GetByIdAsync(int id)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _db.Hackathons, h => h.Id == id
        );
    }

    public async Task<Hackathon?> GetBySlugAsync(string slug)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _db.Hackathons, h => h.Slug == slug
        );
    }

    public async Task<Hackathon> CreateAsync(Hackathon hackathon, string organizerId)
    {
        hackathon.OrganizerId = organizerId;
        hackathon.CreatedAt = DateTime.UtcNow;
        _db.Hackathons.Add(hackathon);
        await _db.SaveChangesAsync();
        return hackathon;
    }

    public async Task UpdateStatusAsync(int id, HackathonStatus newStatus, string actorId)
    {
        var h = await _db.Hackathons.FindAsync(id);
        if (h is not null)
        {
            h.Status = newStatus;
            await _db.SaveChangesAsync();
        }
    }
}
