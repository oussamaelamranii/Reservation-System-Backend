using System.ComponentModel.DataAnnotations;

namespace Reservation_System_Backend.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Session Session { get; set; } = null!;
    public User User { get; set; } = null!;
}
