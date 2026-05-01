using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeNest.API.Data;
using RecipeNest.API.DTOs;

namespace RecipeNest.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]   // all profile endpoints require a valid JWT
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db) => _db = db;

    // ── GET /api/profile ──────────────────────────────────────────────────────
    // Returns the logged-in chef's own profile
    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        int chefId = GetChefIdFromToken();

        // LINQ: single record lookup by primary key
        var profile = await _db.ChefProfiles
            .Where(c => c.Id == chefId)
            .Select(c => new ChefDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Bio         = c.Bio,
                ImageUrl    = c.ImageUrl,
                RecipeCount = c.Recipes.Count
            })
            .FirstOrDefaultAsync();

        if (profile == null) return NotFound(new { message = "Profile not found." });

        return Ok(profile);
    }

    // ── PUT /api/profile ──────────────────────────────────────────────────────
    // Update the logged-in chef's own profile
    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        int chefId = GetChefIdFromToken();

        var profile = await _db.ChefProfiles.FindAsync(chefId);

        if (profile == null) return NotFound(new { message = "Profile not found." });

        profile.Name     = dto.Name;
        profile.Bio      = dto.Bio;
        profile.ImageUrl = dto.ImageUrl;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully." });
    }

    private int GetChefIdFromToken()
    {
        var claim = User.FindFirst("ChefId")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
