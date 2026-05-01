using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeNest.API.Models;

/// <summary>
/// A recipe created by a chef.
/// Many Recipes belong to one ChefProfile (FK: ChefId).
/// </summary>
public class Recipe
{
    public int Id { get; set; }

    // Foreign key to ChefProfile
    public int ChefId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Ingredients { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("ChefId")]
    public ChefProfile? Chef { get; set; }
}
