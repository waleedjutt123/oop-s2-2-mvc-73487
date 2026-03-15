using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Premises> Premises => Set<Premises>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Premises>()
            .HasMany(p => p.Inspections)
            .WithOne(i => i.Premises!)
            .HasForeignKey(i => i.PremisesId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Inspection>()
            .HasMany(i => i.FollowUps)
            .WithOne(f => f.Inspection!)
            .HasForeignKey(f => f.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
