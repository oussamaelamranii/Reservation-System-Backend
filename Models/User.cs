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

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
