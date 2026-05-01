using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeNest.API.Data;
using RecipeNest.API.DTOs;
using RecipeNest.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecipeNest.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext  _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    // ── POST /api/auth/register ───────────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // LINQ: check if email already exists
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return BadRequest(new { message = "Email is already registered." });

        // Create user with hashed password (BCrypt)
        var user = new User
        {
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // saves user, gets its Id

        // Create the chef profile linked to this user
        var profile = new ChefProfile
        {
            UserId   = user.Id,
            Name     = dto.Name,
            Bio      = dto.Bio,
            ImageUrl = dto.ImageUrl
        };
        _db.ChefProfiles.Add(profile);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user, profile.Id);

        return Ok(new AuthResponseDto
        {
            Token  = token,
            Email  = user.Email,
            ChefId = profile.Id,
            Name   = profile.Name
        });
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // LINQ: fetch user and include their chef profile in one query
        var user = await _db.Users
            .Include(u => u.ChefProfile)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = GenerateJwtToken(user, user.ChefProfile!.Id);

        return Ok(new AuthResponseDto
        {
            Token  = token,
            Email  = user.Email,
            ChefId = user.ChefProfile.Id,
            Name   = user.ChefProfile.Name
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string GenerateJwtToken(User user, int chefId)
    {
        // Claims embedded inside the token – readable by the frontend and API
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("ChefId", chefId.ToString())
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
