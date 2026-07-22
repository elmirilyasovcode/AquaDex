namespace AquaDex.Core.DTOs;

public class PointsSummaryDto
{
    public int TotalPoints { get; set; }
    public List<PointsTransactionDto> RecentTransactions { get; set; } = new();
}

public class PointsTransactionDto
{
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
public class BadgeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Earned { get; set; }
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Points { get; set; }
}