using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;

namespace AquaDex.Infrastructure.Services;

public class AuditService
{
    private readonly AquaDexDbContext _context;

    public AuditService(AquaDexDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string userId, string action, string entityType, string? entityId = null, string? details = null)
    {
        _context.AuditLogEntries.Add(new AuditLogEntry
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}