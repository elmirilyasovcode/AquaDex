using System;
using System.Collections.Generic;
using System.Text;
using AquaDex.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Infrastructure.Data
{
    public class AquaDexDbContext : DbContext
    {
        public AquaDexDbContext(DbContextOptions<AquaDexDbContext> options) : base(options)
        {
        }

        public DbSet<Species> Species { get; set; } = null!;
        public DbSet<Waterbody> Waterbodies { get; set; } = null!;
        public DbSet<SpeciesWaterbody> SpeciesWaterbodies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the SpeciesWaterbody join entity relationships
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

            // Prevent the same Species/Waterbody pair from being inserted twice
            modelBuilder.Entity<SpeciesWaterbody>()
                .HasIndex(sw => new { sw.SpeciesId, sw.WaterbodyId })
                .IsUnique();
        }
    }
}
