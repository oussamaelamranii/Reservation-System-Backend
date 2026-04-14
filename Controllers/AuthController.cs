using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_System_Backend.Data;
using Reservation_System_Backend.DTOs;
using Reservation_System_Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Reservation_System_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// Register a new client account — returns a pending confirmation message (no token)
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login with email and password — blocked if account is pending or rejected
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current authenticated user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _authService.GetCurrentUserAsync(userId);

        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        return Ok(user);
    }

    /// <summary>
    /// Request reactivation for an expired subscription (public — user is logged out)
    /// </summary>
    [HttpPost("request-renewal")]
    public async Task<ActionResult> RequestRenewal([FromBody] RequestRenewalDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            return NotFound(new { message = "Utilisateur non trouvé" });

        await _authService.RequestRenewalAsync(user.Id);
        return Ok(new { message = "Demande de réactivation envoyée avec succès" });
    }
}

public class RequestRenewalDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
