using System.ComponentModel.DataAnnotations;

namespace Reservation_System_Backend.DTOs;

// ─── Auth DTOs ───

public class RegisterDto
{
    [Required(ErrorMessage = "Le prénom est requis")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(4, ErrorMessage = "Le mot de passe doit contenir au moins 4 caractères")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>starter | standard | intensif | premium</summary>
    [MaxLength(50)]
    public string? SubscriptionType { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

// ─── User DTO ───

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SubscriptionType { get; set; }
    public int SessionsPerWeek { get; set; }
    public string InscriptionDate { get; set; } = string.Empty;
}

// ─── Session DTOs ───

public class SessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string CoachBio { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // ISO string for frontend
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int BookingsCount { get; set; }
}

public class CreateSessionDto
{
    [Required(ErrorMessage = "Le titre est requis")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom du coach est requis")]
    [MaxLength(200)]
    public string CoachName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CoachBio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date est requise")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "L'heure de début est requise")]
    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
    public string StartTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'heure de fin est requise")]
    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
    public string EndTime { get; set; } = string.Empty;

    [Required]
    [Range(1, 100, ErrorMessage = "La capacité doit être entre 1 et 100")]
    public int Capacity { get; set; }
}

public class UpdateSessionDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? CoachName { get; set; }

    [MaxLength(500)]
    public string? CoachBio { get; set; }

    public DateTime? Date { get; set; }

    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
    public string? StartTime { get; set; }

    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
    public string? EndTime { get; set; }

    [Range(1, 100, ErrorMessage = "La capacité doit être entre 1 et 100")]
    public int? Capacity { get; set; }
}

// ─── Booking DTOs ───

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public SessionDto Session { get; set; } = null!;
}

public class CreateBookingDto
{
    [Required(ErrorMessage = "L'identifiant de la session est requis")]
    public Guid SessionId { get; set; }
}

// ─── Admin DTOs ───

public class AdminStatsDto
{
    public int TotalSessions { get; set; }
    public int TotalClients { get; set; }
    public int TotalBookings { get; set; }
    public int PendingClients { get; set; }
}

public class ApproveUserDto
{
    /// <summary>starter | standard | intensif | premium</summary>
    [Required]
    public string SubscriptionType { get; set; } = string.Empty;

    [Required]
    [Range(1, 99)]
    public int SessionsPerWeek { get; set; }
}
