using System.ComponentModel.DataAnnotations;

namespace RecipeNest.API.Models;

/// <summary>
/// Represents an authenticated user account.
/// One User has one ChefProfile (1-to-1 relationship).
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation property – EF Core uses this to build the FK relationship
    public ChefProfile? ChefProfile { get; set; }
}
