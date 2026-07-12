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
    }
}