using System.ComponentModel.DataAnnotations;

namespace RecipeNest.API.DTOs;

// Returned when reading recipes
public class RecipeDto
{
    public int      Id           { get; set; }
    public int      ChefId       { get; set; }
    public string   ChefName     { get; set; } = string.Empty;
    public string   Title        { get; set; } = string.Empty;
    public string   ImageUrl     { get; set; } = string.Empty;
    public string   Ingredients  { get; set; } = string.Empty;
    public string   Instructions { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
}

// Used when creating a new recipe (POST)
public class CreateRecipeDto
{
    [Required, MaxLength(200)]
    public string Title        { get; set; } = string.Empty;

    [Required]
    public string ImageUrl     { get; set; } = string.Empty;

    [Required]
    public string Ingredients  { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;
}

// Used when editing an existing recipe (PUT)
public class UpdateRecipeDto
{
    [Required, MaxLength(200)]
    public string Title        { get; set; } = string.Empty;

    [Required]
    public string ImageUrl     { get; set; } = string.Empty;

    [Required]
    public string Ingredients  { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;
}
