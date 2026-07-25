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
    public DbSet<ForumReplyVote> ForumReplyVotes { get; set; } = null!;
    public DbSet<ForumThreadSpeciesTag> ForumThreadSpeciesTags { get; set; } = null!;
    public DbSet<ForumThreadWaterbodyTag> ForumThreadWaterbodyTags { get; set; } = null!;

    public DbSet<PointsTransaction> PointsTransactions { get; set; } = null!;
    public DbSet<ExpertConsultation> ExpertConsultations { get; set; } = null!;
    public DbSet<GuideListing> GuideListings { get; set; } = null!;
    public DbSet<GuideBooking> GuideBookings { get; set; } = null!;
    public DbSet<UserNotification> UserNotifications { get; set; } = null!;
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; } = null!;
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
        modelBuilder.Entity<ForumReplyVote>()
            .HasOne(v => v.Reply)
            .WithMany()
            .HasForeignKey(v => v.ReplyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumReplyVote>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ForumReplyVote>()
            .HasIndex(v => new { v.ReplyId, v.UserId })
            .IsUnique();
        modelBuilder.Entity<ForumThreadSpeciesTag>()
            .HasOne(t => t.Thread)
            .WithMany()
            .HasForeignKey(t => t.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumThreadSpeciesTag>()
            .HasOne(t => t.Species)
            .WithMany()
            .HasForeignKey(t => t.SpeciesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumThreadSpeciesTag>()
            .HasIndex(t => new { t.ThreadId, t.SpeciesId })
            .IsUnique();

        modelBuilder.Entity<ForumThreadWaterbodyTag>()
            .HasOne(t => t.Thread)
            .WithMany()
            .HasForeignKey(t => t.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumThreadWaterbodyTag>()
            .HasOne(t => t.Waterbody)
            .WithMany()
            .HasForeignKey(t => t.WaterbodyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForumThreadWaterbodyTag>()
            .HasIndex(t => new { t.ThreadId, t.WaterbodyId })
            .IsUnique();

        modelBuilder.Entity<PointsTransaction>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ExpertConsultation>()
            .HasOne(c => c.RequesterUser)
            .WithMany()
            .HasForeignKey(c => c.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpertConsultation>()
            .HasOne(c => c.ExpertUser)
            .WithMany()
            .HasForeignKey(c => c.ExpertUserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GuideListing>()
            .HasOne(g => g.GuideUser)
            .WithMany()
            .HasForeignKey(g => g.GuideUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GuideBooking>()
            .HasOne(b => b.Listing)
            .WithMany()
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GuideBooking>()
            .HasOne(b => b.RequesterUser)
            .WithMany()
            .HasForeignKey(b => b.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GuideListing>()
            .Property(g => g.PricePerDay)
            .HasPrecision(8, 2);
        modelBuilder.Entity<UserNotification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}