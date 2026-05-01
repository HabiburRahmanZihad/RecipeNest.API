using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeNest.API.Data;
using RecipeNest.API.DTOs;

namespace RecipeNest.API.Controllers;

[ApiController]
[Route("api/chefs")]
public class ChefsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChefsController(AppDbContext db) => _db = db;

    // ── GET /api/chefs ────────────────────────────────────────────────────────
    // LINQ: project ChefProfile into ChefDto, include recipe count
    [HttpGet]
    public async Task<IActionResult> GetAllChefs()
    {
        var chefs = await _db.ChefProfiles
            .Select(c => new ChefDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Bio         = c.Bio,
                ImageUrl    = c.ImageUrl,
                RecipeCount = c.Recipes.Count   // EF translates to a subquery
            })
            .OrderBy(c => c.Name)               // LINQ ordering
            .ToListAsync();

        return Ok(chefs);
    }

    // ── GET /api/chefs/{id} ───────────────────────────────────────────────────
    // LINQ: fetch one chef with their full recipe list
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChefById(int id)
    {
        var chef = await _db.ChefProfiles
            .Where(c => c.Id == id)             // LINQ Where filter
            .Select(c => new ChefDetailDto
            {
                Id       = c.Id,
                Name     = c.Name,
                Bio      = c.Bio,
                ImageUrl = c.ImageUrl,
                Recipes  = c.Recipes
                    .OrderByDescending(r => r.CreatedAt)  // LINQ sort
                    .Select(r => new RecipeDto
                    {
                        Id           = r.Id,
                        ChefId       = r.ChefId,
                        ChefName     = c.Name,
                        Title        = r.Title,
                        Ingredients  = r.Ingredients,
                        Instructions = r.Instructions,
                        CreatedAt    = r.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (chef == null) return NotFound(new { message = "Chef not found." });

        return Ok(chef);
    }
}
