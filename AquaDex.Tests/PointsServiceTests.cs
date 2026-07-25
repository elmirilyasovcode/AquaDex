using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AquaDex.Tests;

public class PointsServiceTests
{
    private static AquaDexDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AquaDexDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test, avoids test cross-contamination
            .Options;

        return new AquaDexDbContext(options);
    }

    [Fact]
    public async Task AwardPointsAsync_CatchLogged_CreatesTransactionWithCorrectPoints()
    {
        using var context = CreateInMemoryContext();
        var service = new PointsService(context);

        await service.AwardPointsAsync("test-user-1", PointsReason.CatchLogged, referenceId: 1);

        var transaction = await context.PointsTransactions.FirstOrDefaultAsync();

        Assert.NotNull(transaction);
        Assert.Equal("test-user-1", transaction.UserId);
        Assert.Equal(5, transaction.Points); // matches your PointValues dictionary
        Assert.Equal(PointsReason.CatchLogged, transaction.Reason);
    }

    [Fact]
    public async Task AwardPointsAsync_MultipleAwards_SumsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var service = new PointsService(context);

        await service.AwardPointsAsync("test-user-1", PointsReason.CatchLogged);
        await service.AwardPointsAsync("test-user-1", PointsReason.ForumThreadPosted);
        await service.AwardPointsAsync("test-user-1", PointsReason.ForumBestAnswer);

        var total = await context.PointsTransactions
            .Where(p => p.UserId == "test-user-1")
            .SumAsync(p => p.Points);

        Assert.Equal(17, total); // 5 + 2 + 10
    }
}