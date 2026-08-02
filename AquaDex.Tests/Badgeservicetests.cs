using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AquaDex.Tests;

public class BadgeServiceTests
{
    private static AquaDexDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AquaDexDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AquaDexDbContext(options);
    }

    private static Species MakeSpecies(int id) => new()
    {
        Id = id,
        CommonNameAz = "Test",
        CommonNameEn = $"Species{id}",
        LatinName = "Testus",
        HabitatType = HabitatType.River,
        MinSizeCm = 1,
        MaxSizeCm = 10,
        Diet = "x",
        ConservationStatus = ConservationStatus.LeastConcern,
        BestBaitTechnique = "x",
        LegalSeasonNotes = "x"
    };

    [Fact]
    public async Task GetBadgesForUserAsync_NoCatches_FirstCatchBadgeNotEarned()
    {
        using var context = CreateInMemoryContext();
        var service = new BadgeService(context);

        var badges = await service.GetBadgesForUserAsync("user-with-no-catches");

        var firstCatch = badges.First(b => b.Name == "First Catch");
        Assert.False(firstCatch.Earned);
    }

    [Fact]
    public async Task GetBadgesForUserAsync_OneCatch_FirstCatchBadgeEarned()
    {
        using var context = CreateInMemoryContext();
        var species = MakeSpecies(1);
        context.Species.Add(species);
        context.CatchLogs.Add(new CatchLog
        {
            UserId = "user-1",
            SpeciesId = 1,
            Species = species,
            User = new ApplicationUser { Id = "user-1", DisplayName = "Test" },
            CaughtAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new BadgeService(context);
        var badges = await service.GetBadgesForUserAsync("user-1");

        Assert.True(badges.First(b => b.Name == "First Catch").Earned);
        Assert.False(badges.First(b => b.Name == "Dedicated Angler").Earned); // needs 10, only has 1
    }

    [Fact]
    public async Task GetBadgesForUserAsync_TenCatches_DedicatedAnglerBadgeEarned()
    {
        using var context = CreateInMemoryContext();
        var species = MakeSpecies(1);
        var user = new ApplicationUser { Id = "user-1", DisplayName = "Test" };
        context.Species.Add(species);

        for (int i = 0; i < 10; i++)
        {
            context.CatchLogs.Add(new CatchLog
            {
                UserId = "user-1",
                SpeciesId = 1,
                Species = species,
                User = user,
                CaughtAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var service = new BadgeService(context);
        var badges = await service.GetBadgesForUserAsync("user-1");

        Assert.True(badges.First(b => b.Name == "Dedicated Angler").Earned);
    }

    [Fact]
    public async Task GetBadgesForUserAsync_AllSpeciesDiscovered_EncyclopedistBadgeEarned()
    {
        using var context = CreateInMemoryContext();
        var user = new ApplicationUser { Id = "user-1", DisplayName = "Test" };
        var species1 = MakeSpecies(1);
        var species2 = MakeSpecies(2);
        context.Species.AddRange(species1, species2);
        context.CatchLogs.AddRange(
            new CatchLog { UserId = "user-1", SpeciesId = 1, Species = species1, User = user, CaughtAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new CatchLog { UserId = "user-1", SpeciesId = 2, Species = species2, User = user, CaughtAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new BadgeService(context);
        var badges = await service.GetBadgesForUserAsync("user-1");

        Assert.True(badges.First(b => b.Name == "Encyclopedist").Earned);
    }

    [Fact]
    public async Task GetBadgesForUserAsync_ConfirmedViolationReport_SturgeonGuardianBadgeEarned()
    {
        using var context = CreateInMemoryContext();
        context.RuleViolationReports.Add(new RuleViolationReport
        {
            ReportedByUserId = "user-1",
            ReportedByUser = new ApplicationUser { Id = "user-1", DisplayName = "Test" },
            ViolationType = ViolationType.Poaching,
            Description = "test",
            Latitude = 40,
            Longitude = 49,
            Status = ReportStatus.Confirmed,
            ReportedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new BadgeService(context);
        var badges = await service.GetBadgesForUserAsync("user-1");

        Assert.True(badges.First(b => b.Name == "Sturgeon Guardian").Earned);
    }

    [Fact]
    public async Task GetBadgesForUserAsync_PendingViolationReport_SturgeonGuardianBadgeNotEarned()
    {
        using var context = CreateInMemoryContext();
        context.RuleViolationReports.Add(new RuleViolationReport
        {
            ReportedByUserId = "user-1",
            ReportedByUser = new ApplicationUser { Id = "user-1", DisplayName = "Test" },
            ViolationType = ViolationType.Poaching,
            Description = "test",
            Latitude = 40,
            Longitude = 49,
            Status = ReportStatus.Pending, // not yet reviewed
            ReportedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new BadgeService(context);
        var badges = await service.GetBadgesForUserAsync("user-1");

        Assert.False(badges.First(b => b.Name == "Sturgeon Guardian").Earned);
    }
}