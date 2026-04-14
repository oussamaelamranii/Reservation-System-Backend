using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_System_Backend.Data;
using Reservation_System_Backend.DTOs;
using Reservation_System_Backend.Services;

namespace Reservation_System_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Get dashboard statistics</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        var stats = new AdminStatsDto
        {
            TotalSessions  = await _context.Sessions.CountAsync(),
            TotalClients   = await _context.Users.CountAsync(u => u.Role == "client" && u.Status == "approved"),
            TotalBookings  = await _context.Bookings.CountAsync(),
            PendingClients = await _context.Users.CountAsync(u => u.Status == "pending")
        };

        return Ok(stats);
    }

    /// <summary>Get all users (clients + admins)</summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.Status)   // pending first
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserDto
            {
                Id               = u.Id,
                FirstName        = u.FirstName,
                LastName         = u.LastName,
                Email            = u.Email,
                Role             = u.Role,
                Status           = u.Status,
                SubscriptionType = u.SubscriptionType,
                SessionsPerWeek  = u.SessionsPerWeek,
                InscriptionDate  = u.CreatedAt.ToString("o")
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>Approve a pending client — set their subscription details</summary>
    [HttpPost("users/{id:guid}/approve")]
    public async Task<ActionResult<UserDto>> ApproveUser(Guid id, [FromBody] ApproveUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        user.Status           = "approved";
        user.SubscriptionType = dto.SubscriptionType.ToLower();
        user.SessionsPerWeek  = dto.SessionsPerWeek;
        user.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);

        await _context.SaveChangesAsync();

        return Ok(AuthService.MapToDto(user));
    }

    /// <summary>Reject a pending client</summary>
    [HttpPost("users/{id:guid}/reject")]
    public async Task<ActionResult<UserDto>> RejectUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        user.Status = "rejected";
        await _context.SaveChangesAsync();

        return Ok(AuthService.MapToDto(user));
    }

    /// <summary>Delete a user entirely</summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get all users who requested an abonnement renewal</summary>
    [HttpGet("renewals")]
    public async Task<ActionResult<List<UserDto>>> GetRenewals()
    {
        var users = await _context.Users
            .Where(u => u.RenewalRequested)
            .OrderByDescending(u => u.SubscriptionExpiresAt)
            .Select(u => AuthService.MapToDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>Approve a renewal request — extend by 1 month</summary>
    [HttpPost("users/{id:guid}/approve-renewal")]
    public async Task<ActionResult<UserDto>> ApproveRenewal(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        user.RenewalRequested = false;
        // Extend from today or from current expiry (whichever is later)
        var baseDate = (user.SubscriptionExpiresAt.HasValue && user.SubscriptionExpiresAt.Value > DateTime.UtcNow)
            ? user.SubscriptionExpiresAt.Value
            : DateTime.UtcNow;
            
        user.SubscriptionExpiresAt = baseDate.AddMonths(1);

        await _context.SaveChangesAsync();

        return Ok(AuthService.MapToDto(user));
    }

    /// <summary>Reject a renewal request — clear the flag, leave user as-is (expired)</summary>
    [HttpPost("users/{id:guid}/reject-renewal")]
    public async Task<ActionResult<UserDto>> RejectRenewal(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        user.RenewalRequested = false;
        await _context.SaveChangesAsync();

        return Ok(AuthService.MapToDto(user));
    }
}
