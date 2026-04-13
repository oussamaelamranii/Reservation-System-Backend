using System.ComponentModel.DataAnnotations;

namespace Reservation_System_Backend.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "client"; // "client" or "admin"

    /// <summary>pending | approved | rejected</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "pending";

    /// <summary>starter | standard | intensif | premium</summary>
    [MaxLength(50)]
    public string? SubscriptionType { get; set; }

    /// <summary>Max sessions allowed per week (0 = unlimited)</summary>
    public int SessionsPerWeek { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
