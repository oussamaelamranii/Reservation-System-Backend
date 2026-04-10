using System.ComponentModel.DataAnnotations;

namespace Reservation_System_Backend.Models;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CoachName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CoachBio { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required, MaxLength(5)] // HH:mm
    public string StartTime { get; set; } = string.Empty;

    [Required, MaxLength(5)] // HH:mm
    public string EndTime { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
