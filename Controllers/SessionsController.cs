using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_System_Backend.Data;
using Reservation_System_Backend.DTOs;
using Reservation_System_Backend.Models;

namespace Reservation_System_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all sessions with booking counts (public)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SessionDto>>> GetAll()
    {
        var sessions = await _context.Sessions
            .Include(s => s.Bookings)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Get a single session by ID (public)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDto>> GetById(Guid id)
    {
        var session = await _context.Sessions
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Session non trouvée" });

        return Ok(MapToDto(session));
    }

    /// <summary>
    /// Create a new session (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionDto dto)
    {
        var session = new Session
        {
            Title = dto.Title,
            Description = dto.Description,
            CoachName = dto.CoachName,
            CoachBio = dto.CoachBio,
            Date = dto.Date.Date, // Ensure date only
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = session.Id }, MapToDto(session));
    }

    /// <summary>
    /// Update a session (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SessionDto>> Update(Guid id, [FromBody] UpdateSessionDto dto)
    {
        var session = await _context.Sessions
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Session non trouvée" });

        // Only update non-null fields
        if (dto.Title != null) session.Title = dto.Title;
        if (dto.Description != null) session.Description = dto.Description;
        if (dto.CoachName != null) session.CoachName = dto.CoachName;
        if (dto.CoachBio != null) session.CoachBio = dto.CoachBio;
        if (dto.Date.HasValue) session.Date = dto.Date.Value.Date;
        if (dto.StartTime != null) session.StartTime = dto.StartTime;
        if (dto.EndTime != null) session.EndTime = dto.EndTime;
        if (dto.Capacity.HasValue) session.Capacity = dto.Capacity.Value;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(session));
    }

    /// <summary>
    /// Delete a session and all its bookings (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var session = await _context.Sessions.FindAsync(id);

        if (session == null)
            return NotFound(new { message = "Session non trouvée" });

        _context.Sessions.Remove(session); // Cascade deletes bookings
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get all users who booked a specific session (Admin only)
    /// </summary>
    [HttpGet("{id:guid}/bookings")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> GetSessionBookings(Guid id)
    {
        var session = await _context.Sessions
            .Include(s => s.Bookings)
            .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Session non trouvée" });

        var users = session.Bookings.Select(b => new
        {
            BookingId = b.Id,
            CreatedAt = b.CreatedAt.ToString("o"),
            User = new UserDto
            {
                Id = b.User.Id,
                FirstName = b.User.FirstName,
                LastName = b.User.LastName,
                Email = b.User.Email,
                Role = b.User.Role,
                Status = b.User.Status,
                SubscriptionType = b.User.SubscriptionType,
                SessionsPerWeek = b.User.SessionsPerWeek,
                InscriptionDate = b.User.CreatedAt.ToString("o")
            }
        }).ToList();

        return Ok(users);
    }

    private static SessionDto MapToDto(Session s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        CoachName = s.CoachName,
        CoachBio = s.CoachBio,
        Date = s.Date.ToString("o"), // ISO 8601 for frontend parseISO()
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Capacity = s.Capacity,
        BookingsCount = s.Bookings?.Count ?? 0
    };
}
