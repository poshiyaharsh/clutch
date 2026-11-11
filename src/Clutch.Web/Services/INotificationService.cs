using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public interface INotificationService
{
    Task SendAsync(string userId, string title, string body, string? url = null);
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int limit = 20);
    Task MarkAsReadAsync(int notificationId, string userId);
}

public class NotificationService : INotificationService
{
    private readonly Data.AppDbContext _db;

    public NotificationService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task SendAsync(string userId, string title, string body, string? url = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int limit = 20)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (n is not null && !n.IsRead)
        {
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
