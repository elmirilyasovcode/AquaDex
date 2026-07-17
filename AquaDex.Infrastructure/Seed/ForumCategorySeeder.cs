using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Infrastructure.Seed;

public static class ForumCategorySeeder
{
    public static async Task SeedCategoriesAsync(AquaDexDbContext context)
    {
        if (await context.ForumCategories.AnyAsync())
            return; // already seeded, don't duplicate

        var categories = new List<ForumCategory>
        {
            new() { Name = "ID Help", Description = "What fish is this? Post a photo and get help identifying it.", SortOrder = 1 },
            new() { Name = "Gear Discussion", Description = "Rods, reels, bait, tackle — talk gear.", SortOrder = 2 },
            new() { Name = "Location Reports", Description = "Share how fishing spots are performing.", SortOrder = 3 },
            new() { Name = "Rules & Regulations", Description = "Legal seasons, size limits, protected species rules.", SortOrder = 4 },
            new() { Name = "General", Description = "Everything else.", SortOrder = 5 }
        };

        context.ForumCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}