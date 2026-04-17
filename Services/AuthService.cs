using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reservation_System_Backend.Data;
using Reservation_System_Backend.DTOs;
using Reservation_System_Backend.Models;

namespace Reservation_System_Backend.Services;

public interface IAuthService
{
    Task<object> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task RequestRenewalAsync(Guid userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    // Subscription → sessions per week mapping
    private static readonly Dictionary<string, int> SessionLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        { "starter",  2 },
        { "standard", 3 },
        { "intensif", 4 },
        { "premium",  0 } // 0 = unlimited
    };

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new client — account is PENDING until admin approves
    /// </summary>
    public async Task<object> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Cet email est déjà utilisé");

        var sessionsPerWeek = dto.SubscriptionType != null && SessionLimits.TryGetValue(dto.SubscriptionType, out var limit)
            ? limit
            : 0;

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "client",
            Status = "pending",          // ← requires admin approval
            SubscriptionType = dto.SubscriptionType?.ToLower(),
            SessionsPerWeek = sessionsPerWeek,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Return a plain message — no JWT token until approved
        return new { message = "Inscription reçue. Votre compte sera activé après validation par l'administrateur." };
    }

    /// <summary>
    /// Login — blocked for pending/rejected accounts
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Identifiants incorrects");

        if (user.Status == "pending")
            throw new UnauthorizedAccessException("Votre compte est en attente de validation par l'administrateur.");

        if (user.Status == "rejected")
            throw new UnauthorizedAccessException("Votre compte a été refusé. Contactez l'administrateur pour plus d'informations.");

        // Check subscription expiry (1-month rule)
        if (user.Role == "client" && user.SubscriptionExpiresAt.HasValue && user.SubscriptionExpiresAt.Value < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("SUBSCRIPTION_EXPIRED: Votre abonnement est terminé. Veuillez demander une réactivation.");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = MapToDto(user)
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task RequestRenewalAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        user.RenewalRequested = true;
        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(jwtSettings["ExpiryInDays"] ?? "7")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    internal static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role,
        Status = user.Status,
        SubscriptionType = user.SubscriptionType,
        SessionsPerWeek = user.SessionsPerWeek,
        InscriptionDate = user.CreatedAt.ToString("o"),
        SubscriptionExpiresAt = user.SubscriptionExpiresAt?.ToString("o"),
        RenewalRequested = user.RenewalRequested
    };
}
