using AquaDex.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Infrastructure.Data;

public class AquaDexDbContext : IdentityDbContext<ApplicationUser>
{
    public AquaDexDbContext(DbContextOptions<AquaDexDbContext> options) : base(options)
    {
    }

    public DbSet<Species> Species { get; set; } = null!;
    public DbSet<Waterbody> Waterbodies { get; set; } = null!;
    public DbSet<SpeciesWaterbody> SpeciesWaterbodies { get; set; } = null!;
    public DbSet<CatchLog> CatchLogs { get; set; } = null!;
    public DbSet<RuleViolationReport> RuleViolationReports { get; set; } = null!;
    public DbSet<BiteAlert> BiteAlerts { get; set; } = null!;
    public DbSet<ForumCategory> ForumCategories { get; set; } = null!;
    public DbSet<ForumThread> ForumThreads { get; set; } = null!;
    public DbSet<ForumReply> ForumReplies { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // IMPORTANT: must be called for Identity tables to be configured correctly

        modelBuilder.Entity<SpeciesWaterbody>()
            .HasOne(sw => sw.Species)
            .WithMany(s => s.SpeciesWaterbodies)
            .HasForeignKey(sw => sw.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SpeciesWaterbody>()
            .HasOne(sw => sw.Waterbody)
            .WithMany(w => w.SpeciesWaterbodies)
            .HasForeignKey(sw => sw.WaterbodyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SpeciesWaterbody>()
            .HasIndex(sw => new { sw.SpeciesId, sw.WaterbodyId })
            .IsUnique();

        modelBuilder.Entity<CatchLog>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CatchLog>()
            .HasOne(c => c.Species)
            .WithMany()
            .HasForeignKey(c => c.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RuleViolationReport>()
            .HasOne(r => r.ReportedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RuleViolationReport>()
            .HasOne(r => r.Waterbody)
            .WithMany()
            .HasForeignKey(r => r.WaterbodyId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BiteAlert>()
            .HasOne(b => b.PostedByUser)
            .WithMany()
            .HasForeignKey(b => b.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BiteAlert>()
            .HasOne(b => b.Species)
            .WithMany()
            .HasForeignKey(b => b.SpeciesId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<ForumThread>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Threads)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ForumThread>()
            .HasOne(t => t.AuthorUser)
            .WithMany()
            .HasForeignKey(t => t.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ForumReply>()
            .HasOne(r => r.Thread)
            .WithMany(t => t.Replies)
            .HasForeignKey(r => r.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumReply>()
            .HasOne(r => r.AuthorUser)
            .WithMany()
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}