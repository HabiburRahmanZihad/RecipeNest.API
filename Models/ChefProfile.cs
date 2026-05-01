using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeNest.API.Models;

/// <summary>
/// Public-facing profile of a chef.
/// One ChefProfile belongs to one User (FK: UserId).
/// One ChefProfile has many Recipes (1-to-many).
/// </summary>
public class ChefProfile
{
    public int Id { get; set; }

    // Foreign key to User
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey("UserId")]
    public User? User { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
