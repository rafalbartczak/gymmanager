using GymManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<PassType> PassTypes => Set<PassType>();
    public DbSet<Pass> Passes => Set<Pass>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ClassType> ClassTypes => Set<ClassType>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<ClassReservation> ClassReservations => Set<ClassReservation>();
    public DbSet<Entry> Entries => Set<Entry>();
}