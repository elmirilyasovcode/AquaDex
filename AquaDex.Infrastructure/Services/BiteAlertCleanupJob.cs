using AquaDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Infrastructure.Services;

public class BiteAlertCleanupJob
{
    private readonly AquaDexDbContext _context;

    public BiteAlertCleanupJob(AquaDexDbContext context)
    {
        _context = context;
    }

    public async Task RunAsync()
    {
        var expired = await _context.BiteAlerts
            .Where(b => b.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Any())
        {
            _context.BiteAlerts.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}