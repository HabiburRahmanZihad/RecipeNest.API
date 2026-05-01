namespace RecipeNest.API.DTOs;

// Returned when listing all chefs or fetching one chef
public class ChefDto
{
    public int    Id       { get; set; }
    public string Name     { get; set; } = string.Empty;
    public string Bio      { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int    RecipeCount { get; set; }
}

// Full chef profile including their recipes (used on the chef detail page)
public class ChefDetailDto
{
    public int              Id        { get; set; }
    public string           Name      { get; set; } = string.Empty;
    public string           Bio       { get; set; } = string.Empty;
    public string           ImageUrl  { get; set; } = string.Empty;
    public List<RecipeDto>  Recipes   { get; set; } = new();
}

// DTO used when a chef updates their own profile
public class UpdateProfileDto
{
    public string Name     { get; set; } = string.Empty;
    public string Bio      { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
