using Microsoft.AspNetCore.Identity;

namespace AquaDex.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Codex completion tracking will hang off this user later (Day 7+)
}