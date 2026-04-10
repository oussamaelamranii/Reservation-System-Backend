using Microsoft.EntityFrameworkCore;
using Reservation_System_Backend.Models;

namespace Reservation_System_Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue("client");
        });

        // Session
        modelBuilder.Entity<Session>(entity =>
        {
            entity.Property(s => s.Capacity).HasDefaultValue(10);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Session)
                  .WithMany(s => s.Bookings)
                  .HasForeignKey(b => b.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Prevent double booking: one user can only book a session once
            entity.HasIndex(b => new { b.SessionId, b.UserId }).IsUnique();
        });
    }
}
