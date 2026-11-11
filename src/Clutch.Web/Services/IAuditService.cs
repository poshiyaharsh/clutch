using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public interface IAuditService
{
    Task LogAsync(string? actorId, string action, string entityType, string entityId, string? details = null);
    Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int limit = 50);
}

public class AuditService : IAuditService
{
    private readonly Data.AppDbContext _db;

    public AuditService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string? actorId, string action, string entityType, string entityId, string? details = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int limit = 50)
    {
        return await _db.AuditLogs
            .Include(a => a.Actor)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
