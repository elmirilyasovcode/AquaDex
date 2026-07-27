using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaDex.Infrastructure.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;

        private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateRemindersAsync();
            }
            catch (Exception ex)
            {
                                                _logger.LogError(ex, "Reminder generation failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task GenerateRemindersAsync()
    {
                        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AquaDexDbContext>();

        await GenerateMissingCodexRemindersAsync(context);

        await context.SaveChangesAsync();
    }

    private async Task GenerateMissingCodexRemindersAsync(AquaDexDbContext context)
    {
        var totalSpeciesCount = await context.Species.CountAsync();
        if (totalSpeciesCount == 0) return;

        var users = await context.Users.ToListAsync();

        foreach (var user in users)
        {
            var discoveredCount = await context.CatchLogs
                .Where(c => c.UserId == user.Id)
                .Select(c => c.SpeciesId)
                .Distinct()
                .CountAsync();

                                    if (discoveredCount == 0 || discoveredCount >= totalSpeciesCount)
                continue;

                                    var recentReminderExists = await context.UserNotifications
                .AnyAsync(n => n.UserId == user.Id
                    && n.Type == NotificationType.MissingCodexSpecies
                    && n.CreatedAt > DateTime.UtcNow.AddDays(-7));

            if (recentReminderExists) continue;

            var remaining = totalSpeciesCount - discoveredCount;
            context.UserNotifications.Add(new UserNotification
            {
                UserId = user.Id,
                Type = NotificationType.MissingCodexSpecies,
                Message = $"You're close! {remaining} species still missing from your Codex.",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}