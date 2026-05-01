using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeNest.API.Data;
using RecipeNest.API.DTOs;
using RecipeNest.API.Models;
using System.Security.Claims;

namespace RecipeNest.API.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RecipesController(AppDbContext db) => _db = db;

    // ── GET /api/recipes ──────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAllRecipes()
    {
        var recipes = await _db.Recipes
            .Include(r => r.Chef)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RecipeDto
            {
                Id           = r.Id,
                ChefId       = r.ChefId,
                ChefName     = r.Chef!.Name,
                Title        = r.Title,
                ImageUrl     = r.ImageUrl,
                Ingredients  = r.Ingredients,
                Instructions = r.Instructions,
                CreatedAt    = r.CreatedAt
            })
            .ToListAsync();

        return Ok(recipes);
    }

    // ── GET /api/recipes/{id} ─────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipeById(int id)
    {
        var recipe = await _db.Recipes
            .Where(r => r.Id == id)
            .Select(r => new RecipeDto
            {
                Id           = r.Id,
                ChefId       = r.ChefId,
                ChefName     = r.Chef!.Name,
                Title        = r.Title,
                ImageUrl     = r.ImageUrl,
                Ingredients  = r.Ingredients,
                Instructions = r.Instructions,
                CreatedAt    = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (recipe == null) return NotFound(new { message = "Recipe not found." });

        return Ok(recipe);
    }

    // ── GET /api/recipes/chef/{chefId} ────────────────────────────────────────
    [HttpGet("chef/{chefId}")]
    public async Task<IActionResult> GetRecipesByChef(int chefId)
    {
        var recipes = await _db.Recipes
            .Where(r => r.ChefId == chefId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RecipeDto
            {
                Id           = r.Id,
                ChefId       = r.ChefId,
                ChefName     = r.Chef!.Name,
                Title        = r.Title,
                ImageUrl     = r.ImageUrl,
                Ingredients  = r.Ingredients,
                Instructions = r.Instructions,
                CreatedAt    = r.CreatedAt
            })
            .ToListAsync();

        return Ok(recipes);
    }

    // ── POST /api/recipes ─────────────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int chefId = GetChefIdFromToken();

        var recipe = new Recipe
        {
            ChefId       = chefId,
            Title        = dto.Title,
            ImageUrl     = dto.ImageUrl,
            Ingredients  = dto.Ingredients,
            Instructions = dto.Instructions,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, new { recipe.Id });
    }

    // ── PUT /api/recipes/{id} ─────────────────────────────────────────────────
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRecipe(int id, [FromBody] UpdateRecipeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int chefId = GetChefIdFromToken();

        var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.ChefId == chefId);

        if (recipe == null)
            return NotFound(new { message = "Recipe not found or you are not the owner." });

        recipe.Title        = dto.Title;
        recipe.ImageUrl     = dto.ImageUrl;
        recipe.Ingredients  = dto.Ingredients;
        recipe.Instructions = dto.Instructions;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ── DELETE /api/recipes/{id} ──────────────────────────────────────────────
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        int chefId = GetChefIdFromToken();

        var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.ChefId == chefId);

        if (recipe == null)
            return NotFound(new { message = "Recipe not found or you are not the owner." });

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private int GetChefIdFromToken()
    {
        var claim = User.FindFirst("ChefId")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
