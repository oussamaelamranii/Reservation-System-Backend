using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_System_Backend.Data;
using Reservation_System_Backend.DTOs;
using Reservation_System_Backend.Models;
using System.Security.Claims;

namespace Reservation_System_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get current user's bookings with session details
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<BookingDto>>> GetMyBookings()
    {
        var userId = GetCurrentUserId();

        var bookings = await _context.Bookings
            .Include(b => b.Session)
                .ThenInclude(s => s.Bookings)
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.Session.Date)
            .ThenBy(b => b.Session.StartTime)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                SessionId = b.SessionId,
                UserId = b.UserId,
                CreatedAt = b.CreatedAt.ToString("o"),
                Session = new SessionDto
                {
                    Id = b.Session.Id,
                    Title = b.Session.Title,
                    Description = b.Session.Description,
                    CoachName = b.Session.CoachName,
                    CoachBio = b.Session.CoachBio,
                    Date = b.Session.Date.ToString("o"),
                    StartTime = b.Session.StartTime,
                    EndTime = b.Session.EndTime,
                    Capacity = b.Session.Capacity,
                    BookingsCount = b.Session.Bookings.Count
                }
            })
            .ToListAsync();

        return Ok(bookings);
    }

    /// <summary>
    /// Book a session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto dto)
    {
        var userId = GetCurrentUserId();

        // Check session exists
        var session = await _context.Sessions
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

        if (session == null)
            return NotFound(new { message = "Session non trouvée" });

        // Check if already booked
        var existingBooking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.SessionId == dto.SessionId && b.UserId == userId);

        if (existingBooking != null)
            return Conflict(new { message = "Vous avez déjà réservé cette session" });

        // Check capacity
        if (session.Bookings.Count >= session.Capacity)
            return BadRequest(new { message = "Cette session est complète" });

        var booking = new Booking
        {
            SessionId = dto.SessionId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(booking).Reference(b => b.Session).LoadAsync();
        await _context.Entry(booking.Session).Collection(s => s.Bookings).LoadAsync();

        return CreatedAtAction(nameof(GetMyBookings), new BookingDto
        {
            Id = booking.Id,
            SessionId = booking.SessionId,
            UserId = booking.UserId,
            CreatedAt = booking.CreatedAt.ToString("o"),
            Session = new SessionDto
            {
                Id = session.Id,
                Title = session.Title,
                Description = session.Description,
                CoachName = session.CoachName,
                CoachBio = session.CoachBio,
                Date = session.Date.ToString("o"),
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Capacity = session.Capacity,
                BookingsCount = session.Bookings.Count
            }
        });
    }

    /// <summary>
    /// Cancel a booking (owner only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound(new { message = "Réservation non trouvée" });

        // Only the owner or admin can cancel
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (booking.UserId != userId && role != "admin")
            return Forbid();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
