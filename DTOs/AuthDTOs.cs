using System.ComponentModel.DataAnnotations;

namespace RecipeNest.API.DTOs;

// ── Register ─────────────────────────────────────────────────────────────────

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}

// ── Login ─────────────────────────────────────────────────────────────────────

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// ── Response sent back after successful login/register ────────────────────────

public class AuthResponseDto
{
    public string Token    { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public int    ChefId   { get; set; }
    public string Name     { get; set; } = string.Empty;
}
